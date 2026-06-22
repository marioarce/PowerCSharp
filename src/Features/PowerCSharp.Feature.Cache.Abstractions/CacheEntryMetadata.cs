namespace PowerCSharp.Feature.Cache.Abstractions;

/// <summary>
/// Base metadata about a cache entry, providing diagnostic information about the entry's state and lifecycle.
/// </summary>
public abstract class CacheEntryMetadata
{
    /// <summary>
    /// Gets the unique key of the cache entry.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the UTC timestamp when the entry was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Gets the UTC timestamp when the entry was last accessed.
    /// </summary>
    public DateTime LastAccessedUtc { get; protected set; }

    /// <summary>
    /// Gets the UTC timestamp when the entry expires, or null if it doesn't expire.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; }

    /// <summary>
    /// Gets the size of the cache entry in bytes, if available.
    /// </summary>
    public long? SizeBytes { get; }

    /// <summary>
    /// Gets a value indicating whether the cache value was freshly retrieved (cache hit) or not.
    /// </summary>
    public bool IsFresh { get; protected set; }

    /// <summary>
    /// Gets the number of times this entry has been accessed.
    /// </summary>
    public long AccessCount { get; protected set; }

    /// <summary>
    /// Gets the number of cache hits for this entry.
    /// </summary>
    public long HitCount { get; protected set; }

    /// <summary>
    /// Gets the number of cache misses for this entry.
    /// </summary>
    public long MissCount { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the entry is expired.
    /// </summary>
    public bool IsExpired => ExpiresAtUtc.HasValue && DateTime.UtcNow > ExpiresAtUtc.Value;

    /// <summary>
    /// Gets the time remaining until expiration, or null if the entry doesn't expire.
    /// </summary>
    public TimeSpan? TimeToExpire => ExpiresAtUtc.HasValue ? ExpiresAtUtc.Value - DateTime.UtcNow : null;

    /// <summary>
    /// Gets the age of the cache entry (time since creation).
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - CreatedAtUtc;

    /// <summary>
    /// Gets the time since the entry was last accessed.
    /// </summary>
    public TimeSpan TimeSinceLastAccess => DateTime.UtcNow - LastAccessedUtc;

    /// <summary>
    /// Initializes a new instance of <see cref="CacheEntryMetadata"/>.
    /// </summary>
    protected CacheEntryMetadata(
        string key,
        DateTime createdAtUtc,
        DateTime lastAccessedUtc,
        DateTime? expiresAtUtc = null,
        long? sizeBytes = null,
        bool isFresh = false,
        long accessCount = 0,
        long hitCount = 0,
        long missCount = 0)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        CreatedAtUtc = createdAtUtc;
        LastAccessedUtc = lastAccessedUtc;
        ExpiresAtUtc = expiresAtUtc;
        SizeBytes = sizeBytes;
        IsFresh = isFresh;
        AccessCount = accessCount;
        HitCount = hitCount;
        MissCount = missCount;
    }

    /// <summary>
    /// Updates the access statistics for this cache entry.
    /// </summary>
    /// <param name="isHit">Whether this access was a cache hit.</param>
    public void UpdateAccess(bool isHit)
    {
        LastAccessedUtc = DateTime.UtcNow;
        AccessCount++;
        IsFresh = isHit;
        
        if (isHit)
        {
            HitCount++;
        }
        else
        {
            MissCount++;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var status = IsExpired ? "expired" : "active";
        var sizeInfo = SizeBytes.HasValue ? $"{SizeBytes.Value} bytes" : "unknown size";
        var expiryInfo = ExpiresAtUtc.HasValue ? $"expires {ExpiresAtUtc.Value:yyyy-MM-dd HH:mm:ss} UTC" : "no expiry";
        
        return $"CacheEntry '{Key}' ({status}, {sizeInfo}, {expiryInfo})";
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CacheEntryMetadata other && Equals(other);

    /// <inheritdoc />
    public virtual bool Equals(CacheEntryMetadata? other) => 
        other != null && 
        GetType() == other.GetType() &&
        Key == other.Key && 
        CreatedAtUtc == other.CreatedAtUtc;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;

        hash = hash * 31 + GetType().GetHashCode();
        hash = hash * 31 + (Key?.GetHashCode() ?? 0);
        hash = hash * 31 + CreatedAtUtc.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(CacheEntryMetadata? left, CacheEntryMetadata? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(CacheEntryMetadata? left, CacheEntryMetadata? right) => !(left == right);
}
