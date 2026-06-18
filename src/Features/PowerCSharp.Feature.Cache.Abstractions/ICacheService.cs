using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache;

/// <summary>
/// In-memory cache abstraction. Concrete backends are supplied by provider packages
/// (e.g. <c>PowerCSharp.Feature.Cache.BitFaster</c>); a NoOp is used when disabled or unconfigured.
/// Sync-first with async overloads for performance optimization.
/// </summary>
public interface ICacheService : ICacheStore
{
    /// <summary>Attempts to get a cached value by key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">When this method returns, contains the cached value if found; otherwise, the default value.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>True if the value was found; otherwise, false.</returns>
    bool TryGet<T>(string key, out T value, CancellationToken ct = default);

    /// <summary>Gets a cached value by key, or default if not found.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The cached value, or default if not found.</returns>
    T? Get<T>(string key, CancellationToken ct = default);

    /// <summary>Asynchronously gets a cached value by key, or default if not found.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the cached value, or default if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Gets a cached value with detailed result information.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A cache result with the value and diagnostic information.</returns>
    CacheResult<T> GetWithResult<T>(string key, CancellationToken ct = default);

    /// <summary>Asynchronously gets a cached value with detailed result information.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with a cache result containing the value and diagnostic information.</returns>
    Task<CacheResult<T>> GetWithResultAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Sets a value, with an optional time-to-live.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ttl">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    void Set<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>Asynchronously sets a value, with an optional time-to-live.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ttl">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or creates it with <paramref name="factory"/>,
    /// stores it (with optional <paramref name="ttl"/>), and returns it. Implementations should guard
    /// against cache stampedes so concurrent misses for the same key invoke the factory once.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if not cached.</param>
    /// <param name="ttl">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The cached or newly created value.</returns>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null, CancellationToken ct = default);

    /// <summary>
    /// Asynchronous counterpart to <see cref="GetOrCreate{T}"/>: returns the cached value for
    /// <paramref name="key"/>, or awaits <paramref name="factory"/> to create and store it.
    /// Implementations should guard against cache stampedes for the same key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The asynchronous factory function to create the value if not cached.</param>
    /// <param name="ttl">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the cached or newly created value.</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default);
}
