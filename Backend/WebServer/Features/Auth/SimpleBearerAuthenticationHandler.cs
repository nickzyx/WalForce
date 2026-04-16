using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using WebServer.Configuration;

namespace WebServer.Features.Auth;

public sealed class SimpleBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<AuthOptions> authOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var value = authorizationHeader.ToString();
        if (!value.StartsWith($"{SimpleBearerDefaults.AuthenticationScheme} ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = value[SimpleBearerDefaults.AuthenticationScheme.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing bearer token."));
        }

        if (!BearerTokenCodec.TryReadToken(token, _authOptions, out var payload) || payload is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid bearer token."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, payload.Subject),
            new Claim("sub", payload.Subject),
            new Claim(ClaimTypes.Name, payload.Name),
            new Claim(ClaimTypes.Email, payload.Email),
            new Claim(ClaimTypes.Role, payload.Role)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = SimpleBearerDefaults.AuthenticationScheme;
        return Task.CompletedTask;
    }
}
