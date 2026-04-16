using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var runner = new SmokeTestRunner();
await runner.RunAsync();

internal sealed class SmokeTestRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task RunAsync()
    {
        var dataRoot = CreateTempDataCopy();
        await using var server = await StartServerAsync(dataRoot);
        using var http = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5041") };

        await WaitForServerAsync(http);

        await TestSwaggerAsync(http);
        await TestEmployeeFlowAsync(http);
        await TestManagerFlowAsync(http);
        await TestAvailabilityPersistenceAsync(http);

        Console.WriteLine("Smoke tests passed.");
    }

    private static async Task TestEmployeeFlowAsync(HttpClient http)
    {
        var login = await LoginAsync(http, "ava.diaz@walforce.local", "WalForce!123");
        Assert(login.User.Role == "Employee", "Employee login should return the Employee role.");

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var profile = await GetRequiredAsync<ProfileResponse>(http, "/api/me/profile");
        Assert(profile.Email == "ava.diaz@walforce.local", "Employee profile should match the logged-in user.");

        var schedule = await GetRequiredAsync<List<EmployeeShiftResponse>>(http, "/api/me/schedule?from=2026-04-13&to=2026-04-19");
        Assert(schedule.Count > 0, "Employee schedule should return at least one seeded shift.");

        using var forbidden = await http.GetAsync("/api/manager/roster");
        Assert(forbidden.StatusCode == HttpStatusCode.Forbidden, "Employees must not access manager endpoints.");
    }

    private static async Task TestSwaggerAsync(HttpClient http)
    {
        using var swaggerJsonResponse = await http.GetAsync("/swagger/v1/swagger.json");
        swaggerJsonResponse.EnsureSuccessStatusCode();

        var swaggerJson = await swaggerJsonResponse.Content.ReadAsStringAsync();
        Assert(swaggerJson.Contains("\"title\": \"WalForce Backend API\"", StringComparison.Ordinal), "Swagger JSON should expose the WalForce API title.");
        Assert(swaggerJson.Contains("\"/api/auth/login\"", StringComparison.Ordinal), "Swagger JSON should include the auth login endpoint.");

        using var swaggerUiResponse = await http.GetAsync("/swagger/index.html");
        swaggerUiResponse.EnsureSuccessStatusCode();

        var swaggerUi = await swaggerUiResponse.Content.ReadAsStringAsync();
        Assert(swaggerUi.Contains("swagger-ui", StringComparison.OrdinalIgnoreCase), "Swagger UI page should render the Swagger container.");
    }

    private static async Task TestManagerFlowAsync(HttpClient http)
    {
        var login = await LoginAsync(http, "manager@walforce.local", "WalForce!123");
        Assert(login.User.Role == "Manager", "Manager login should return the Manager role.");

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var profile = await GetRequiredAsync<ProfileResponse>(http, "/api/me/profile");
        Assert(profile.Role == "Manager", "Manager profile should match the logged-in user.");

        var roster = await GetRequiredAsync<List<ManagerRosterResponse>>(http, "/api/manager/roster");
        Assert(roster.Count == 3, "Manager roster should return the three seeded employees.");

        var schedule = await GetRequiredAsync<List<ManagerShiftResponse>>(http, "/api/manager/schedule?from=2026-04-13&to=2026-04-26");
        Assert(schedule.Count >= 10, "Manager schedule should return the seeded team shifts in range.");

        using var forbidden = await http.GetAsync("/api/me/schedule?from=2026-04-13&to=2026-04-19");
        Assert(forbidden.StatusCode == HttpStatusCode.Forbidden, "Managers must not access the employee schedule endpoint.");
    }

    private static async Task TestAvailabilityPersistenceAsync(HttpClient http)
    {
        var login = await LoginAsync(http, "ava.diaz@walforce.local", "WalForce!123");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var availability = await GetRequiredAsync<AvailabilityResponse>(http, "/api/me/availability");
        Assert(availability.Days.Count == 7, "Availability should always return all seven days.");

        var updated = availability with
        {
            Notes = "Updated by smoke tests.",
            Days =
            [
                new AvailabilityDayResponse("Monday", [new AvailabilityWindowResponse("07:00:00", "15:00:00")]),
                new AvailabilityDayResponse("Tuesday", []),
                new AvailabilityDayResponse("Wednesday", []),
                new AvailabilityDayResponse("Thursday", []),
                new AvailabilityDayResponse("Friday", []),
                new AvailabilityDayResponse("Saturday", []),
                new AvailabilityDayResponse("Sunday", [])
            ]
        };

        using var putResponse = await http.PutAsJsonAsync("/api/me/availability", updated);
        putResponse.EnsureSuccessStatusCode();

        var refreshed = await GetRequiredAsync<AvailabilityResponse>(http, "/api/me/availability");
        Assert(refreshed.Notes == "Updated by smoke tests.", "Availability PUT should persist updated notes.");
        Assert(refreshed.Days[0].Windows.Count == 1 && refreshed.Days[0].Windows[0].StartTime == "07:00:00", "Availability PUT should persist the Monday window.");
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient http, string email, string password)
    {
        using var response = await http.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return payload ?? throw new InvalidOperationException("Login response was empty.");
    }

    private static async Task<T> GetRequiredAsync<T>(HttpClient http, string path)
    {
        using var response = await http.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return payload ?? throw new InvalidOperationException($"Response payload for '{path}' was empty.");
    }

    private static async Task WaitForServerAsync(HttpClient http)
    {
        var started = false;

        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                using var response = await http.GetAsync("/");
                if (response.IsSuccessStatusCode)
                {
                    started = true;
                    break;
                }
            }
            catch
            {
                // Startup is still in progress.
            }

            await Task.Delay(500);
        }

        Assert(started, "Server did not become ready within 30 seconds.");
    }

    private static async Task<ServerProcess> StartServerAsync(string dataRoot)
    {
        var projectPath = FindPath("Backend", "WebServer", "WebServer.csproj");
        var startInfo = new ProcessStartInfo("dotnet", $"run --no-build --project \"{projectPath}\" --launch-profile http")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["DOTNET_CLI_HOME"] = @"C:\Users\nicho\.codex\memories";
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:5041";
        startInfo.Environment["Auth__SigningKey"] = "WalForceSmokeTestsSigningKey1234567890!";
        startInfo.Environment["Data__RootPath"] = dataRoot;

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the backend process.");
        return await ServerProcess.CreateAsync(process);
    }

    private static string CreateTempDataCopy()
    {
        var source = FindPath("Backend", "WebServer", "Data");
        var destination = Path.Combine(Path.GetTempPath(), $"walforce-data-{Guid.NewGuid():N}");
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
        }

        return destination;
    }

    private static string FindPath(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, Path.Combine(segments));
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException($"Could not locate '{Path.Combine(segments)}' from '{AppContext.BaseDirectory}'.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

internal sealed class ServerProcess : IAsyncDisposable
{
    private readonly Process _process;
    private readonly Task<string> _discardStdOut;
    private readonly Task<string> _discardStdErr;

    private ServerProcess(Process process, Task<string> discardStdOut, Task<string> discardStdErr)
    {
        _process = process;
        _discardStdOut = discardStdOut;
        _discardStdErr = discardStdErr;
    }

    public static Task<ServerProcess> CreateAsync(Process process)
        => Task.FromResult(new ServerProcess(process, process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync()));

    public async ValueTask DisposeAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }

        await _process.WaitForExitAsync();
        await Task.WhenAll(_discardStdOut, _discardStdErr);

        _process.Dispose();
    }
}

internal sealed record LoginRequest(string Email, string Password);

internal sealed record LoginResponse(string AccessToken, AuthenticatedUserResponse User);

internal sealed record AuthenticatedUserResponse(Guid Id, string FirstName, string LastName, string Email, string Role);

internal sealed record ProfileResponse(Guid Id, string FirstName, string LastName, string Email, string Role, string Title);

internal sealed record EmployeeShiftResponse(Guid Id, string Date, string StartTime, string EndTime, string RoleLabel, string? Note);

internal sealed record ManagerRosterResponse(Guid Id, string FirstName, string LastName, string Email, string Title);

internal sealed record ManagerShiftResponse(Guid Id, Guid EmployeeId, string EmployeeName, string Date, string StartTime, string EndTime, string RoleLabel, string? Note);

internal sealed record AvailabilityResponse(string? Notes, List<AvailabilityDayResponse> Days);

internal sealed record AvailabilityDayResponse(string Day, List<AvailabilityWindowResponse> Windows);

internal sealed record AvailabilityWindowResponse(string StartTime, string EndTime);
