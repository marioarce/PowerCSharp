using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache;

namespace PowerCSharp.Feature.Cache.BitFaster;

/// <summary>
/// Simple JSON file-backed <see cref="IDiskCacheService"/> shipped with the BitFaster provider.
/// Entries are serialized to a directory configured via <see cref="DiskCacheOptions.DirectoryPath"/>.
/// </summary>
public sealed class FileDiskCacheService : IDiskCacheService
{
    private readonly string _root;

    /// <summary>Creates the disk cache, ensuring the storage directory exists.</summary>
    public FileDiskCacheService(IOptions<CacheFeatureOptions> options)
    {
        _root = options.Value.Disk.DirectoryPath
            ?? Path.Combine(Path.GetTempPath(), "powercsharp-cache");
        Directory.CreateDirectory(_root);
    }

    /// <inheritdoc />
    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var path = PathFor(key);
        if (!File.Exists(path))
        {
            return default;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer
            .DeserializeAsync<T>(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var path = PathFor(key);
        await using var stream = File.Create(path);
        await JsonSerializer
            .SerializeAsync(stream, value, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private string PathFor(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Path.Combine(_root, Convert.ToHexString(hash) + ".json");
    }
}
