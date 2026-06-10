namespace PowerCSharp.Feature.Cache;

/// <summary>
/// In-memory cache abstraction. Concrete backends are supplied by provider packages
/// (e.g. <c>PowerCSharp.Feature.Cache.BitFaster</c>); a NoOp is used when disabled or unconfigured.
/// </summary>
public interface ICacheService
{
    /// <summary>Attempts to get a cached value by key.</summary>
    bool TryGet<T>(string key, out T value);

    /// <summary>Sets a value, with an optional time-to-live.</summary>
    void Set<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>Removes a cached value by key.</summary>
    void Remove(string key);

    /// <summary>Removes all entries from the cache.</summary>
    void Clear();

    /// <summary>Gets a snapshot of all keys currently stored in the cache.</summary>
    IReadOnlyCollection<string> GetKeys();

    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or creates it with <paramref name="factory"/>,
    /// stores it (with optional <paramref name="ttl"/>), and returns it. Implementations should guard
    /// against cache stampedes so concurrent misses for the same key invoke the factory once.
    /// </summary>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null);

    /// <summary>
    /// Asynchronous counterpart to <see cref="GetOrCreate{T}"/>: returns the cached value for
    /// <paramref name="key"/>, or awaits <paramref name="factory"/> to create and store it.
    /// Implementations should guard against cache stampedes for the same key.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null);
}
