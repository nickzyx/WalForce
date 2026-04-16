using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WebServer.Configuration;

namespace WebServer.Infrastructure.Data;

public sealed class JsonFileDataStore(IOptions<DataOptions> options, IWebHostEnvironment environment)
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new(StringComparer.OrdinalIgnoreCase);

    private readonly string _rootPath = ResolveRootPath(options.Value.RootPath, environment.ContentRootPath);

    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public Task<List<T>> ReadListAsync<T>(string fileName, CancellationToken cancellationToken = default)
        => WithLockAsync(
            fileName,
            async (path, innerToken) =>
            {
                if (!File.Exists(path))
                {
                    return [];
                }

                await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return await JsonSerializer.DeserializeAsync<List<T>>(stream, _serializerOptions, innerToken) ?? [];
            },
            cancellationToken);

    public async Task WriteAsync<T>(string fileName, T payload, CancellationToken cancellationToken = default)
    {
        await WithLockAsync(
            fileName,
            async (path, innerToken) =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                await using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(stream, payload, _serializerOptions, innerToken);

                return true;
            },
            cancellationToken);
    }

    private async Task<T> WithLockAsync<T>(
        string fileName,
        Func<string, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(_rootPath, fileName);
        var fileLock = FileLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));

        await fileLock.WaitAsync(cancellationToken);
        try
        {
            return await action(path, cancellationToken);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private static string ResolveRootPath(string configuredRootPath, string contentRootPath)
        => Path.IsPathRooted(configuredRootPath)
            ? configuredRootPath
            : Path.Combine(contentRootPath, configuredRootPath);
}
