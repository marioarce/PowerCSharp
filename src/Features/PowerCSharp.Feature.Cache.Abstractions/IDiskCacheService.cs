using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache;

/// <summary>
/// Asynchronous disk-backed cache abstraction. Concrete backends are supplied by provider packages
/// (e.g. <c>PowerCSharp.Feature.Cache.Disk</c>); a NoOp is used when disabled or unconfigured.
/// </summary>
public interface IDiskCacheService
{
    /// <summary>Gets a value by key, or <c>default</c> when absent.</summary>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Sets a value by key.</summary>
    ValueTask SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>Gets a value by key with specified cache file kind, or <c>default</c> when absent.</summary>
    ValueTask<T?> GetAsync<T>(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default);

    /// <summary>Sets a value by key with specified cache file kind.</summary>
    ValueTask SetAsync<T>(string key, T value, CacheFileKind fileKind, CancellationToken cancellationToken = default);

    /// <summary>Gets a cache result with value and metadata.</summary>
    ValueTask<CacheResult<T>> GetWithMetadataAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Gets a cache result with value and metadata using specified file kind.</summary>
    ValueTask<CacheResult<T>> GetWithMetadataAsync<T>(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default);

    /// <summary>Gets metadata for a cache entry without retrieving the value.</summary>
    ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Gets metadata for a cache entry without retrieving the value using specified file kind.</summary>
    ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default);
}
