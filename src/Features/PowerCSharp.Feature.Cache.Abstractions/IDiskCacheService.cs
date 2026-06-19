using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.Abstractions;

/// <summary>
/// Asynchronous disk-backed cache abstraction. Concrete backends are supplied by provider packages
/// (e.g. <c>PowerCSharp.Feature.Cache.Disk</c>); a NoOp is used when disabled or unconfigured.
/// Async-first with sync overloads for I/O optimization.
/// </summary>
public interface IDiskCacheService : ICacheStore
{
    /// <summary>Gets a value by key, or <c>default</c> when absent.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task with the cached value, or default when absent.</returns>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Gets a value by key with specified cache file kind, or <c>default</c> when absent.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="fileKind">The type of cache file.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task with the cached value, or default when absent.</returns>
    ValueTask<T?> GetAsync<T>(string key, CacheFileKind fileKind, CancellationToken ct = default);

    /// <summary>Synchronously gets a value by key, or <c>default</c> when absent.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The cached value, or default when absent.</returns>
    T? Get<T>(string key, CancellationToken ct = default);

    /// <summary>Synchronously gets a value by key with specified cache file kind, or <c>default</c> when absent.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="fileKind">The type of cache file.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The cached value, or default when absent.</returns>
    T? Get<T>(string key, CacheFileKind fileKind, CancellationToken ct = default);

    /// <summary>Sets a value by key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default);

    /// <summary>Sets a value by key with specified cache file kind.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="fileKind">The type of cache file.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask SetAsync<T>(string key, T value, CacheFileKind fileKind, CancellationToken ct = default);

    /// <summary>Synchronously sets a value by key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    void Set<T>(string key, T value, CancellationToken ct = default);

    /// <summary>Synchronously sets a value by key with specified cache file kind.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="fileKind">The type of cache file.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    void Set<T>(string key, T value, CacheFileKind fileKind, CancellationToken ct = default);

    /// <summary>Gets a cache result with value and metadata.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task with a cache result containing the value and metadata.</returns>
    ValueTask<CacheResult<T>> GetWithResultAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Gets a cache result with value and metadata using specified file kind.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="fileKind">The type of cache file.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task with a cache result containing the value and metadata.</returns>
    ValueTask<CacheResult<T>> GetWithResultAsync<T>(string key, CacheFileKind fileKind, CancellationToken ct = default);

    /// <summary>Synchronously gets a cache result with value and metadata.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A cache result containing the value and metadata.</returns>
    CacheResult<T> GetWithResult<T>(string key, CancellationToken ct = default);

    /// <summary>Gets metadata for a cache entry without retrieving the value using specified file kind.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="fileKind">The type of cache file.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task with cache entry metadata, or null if not found.</returns>
    ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CacheFileKind fileKind, CancellationToken ct = default);

    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or creates it with <paramref name="factory"/>,
    /// stores it, and returns it. Implementations should guard against cache stampedes.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The asynchronous factory function to create the value if not cached.</param>
    /// <param name="ttl">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task with the cached or newly created value.</returns>
    ValueTask<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or creates it with <paramref name="factory"/>,
    /// stores it with specified file kind, and returns it. Implementations should guard against cache stampedes.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="fileKind">The type of cache file.</param>
    /// <param name="factory">The asynchronous factory function to create the value if not cached.</param>
    /// <param name="ttl">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A value task with the cached or newly created value.</returns>
    ValueTask<T> GetOrCreateAsync<T>(string key, CacheFileKind fileKind, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>
    /// Synchronously returns the cached value for <paramref name="key"/>, or creates it with <paramref name="factory"/>,
    /// stores it, and returns it. Implementations should guard against cache stampedes.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if not cached.</param>
    /// <param name="ttl">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The cached or newly created value.</returns>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null, CancellationToken ct = default);
}
