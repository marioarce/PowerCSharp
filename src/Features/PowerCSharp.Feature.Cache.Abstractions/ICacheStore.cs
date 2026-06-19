using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.Abstractions;

/// <summary>
/// Base interface for cache storage management operations.
/// Provides common cache management functionality that all cache implementations should support.
/// </summary>
public interface ICacheStore
{
    /// <summary>
    /// Removes a cache entry by key.
    /// </summary>
    /// <param name="key">The key of the cache entry to remove.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    void Remove(string key, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously removes a cache entry by key.
    /// </summary>
    /// <param name="key">The key of the cache entry to remove.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes all entries from the cache.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    void Clear(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously removes all entries from the cache.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a snapshot of all keys currently stored in the cache.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only collection of cache keys.</returns>
    IReadOnlyCollection<string> GetKeys(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously gets a snapshot of all keys currently stored in the cache.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with a read-only collection of cache keys.</returns>
    Task<IReadOnlyCollection<string>> GetKeysAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets metadata about a cache entry without retrieving the value.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>Cache entry metadata, or null if the entry doesn't exist.</returns>
    CacheEntryMetadata? GetMetadata(string key, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously gets metadata about a cache entry without retrieving the value.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with cache entry metadata, or null if the entry doesn't exist.</returns>
    Task<CacheEntryMetadata?> GetMetadataAsync(string key, CancellationToken ct = default);
}
