using PowerCSharp.Feature.Cache.Abstractions.Enums;

namespace PowerCSharp.Feature.Cache.Abstractions;

/// <summary>
/// Metadata about an in-memory cache entry, providing memory-specific diagnostic information.
/// </summary>
public sealed class InMemoryCacheEntryMetadata : CacheEntryMetadata
{
    /// <summary>
    /// Gets the estimated memory size of the cache entry in bytes, if available.
    /// </summary>
    public new long? SizeBytes { get; }

    /// <summary>
    /// Gets a value indicating whether this entry is eligible for eviction under memory pressure.
    /// </summary>
    public bool IsEvictable { get; }

    /// <summary>
    /// Gets the priority of this cache entry for eviction decisions.
    /// </summary>
    public CacheEntryPriority Priority { get; }

    /// <summary>
    /// Gets the approximate memory pressure score (higher means more likely to be evicted).
    /// </summary>
    public double MemoryPressureScore { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryCacheEntryMetadata"/>.
    /// </summary>
    public InMemoryCacheEntryMetadata(
        string key,
        DateTime createdAtUtc,
        DateTime lastAccessedUtc,
        DateTime? expiresAtUtc = null,
        long? sizeBytes = null,
        bool isFresh = false,
        long accessCount = 0,
        long hitCount = 0,
        long missCount = 0,
        bool isEvictable = true,
        CacheEntryPriority priority = CacheEntryPriority.Normal,
        double memoryPressureScore = 0.0)
        : base(key, createdAtUtc, lastAccessedUtc, expiresAtUtc, sizeBytes, isFresh, accessCount, hitCount, missCount)
    {
        SizeBytes = sizeBytes;
        IsEvictable = isEvictable;
        Priority = priority;
        MemoryPressureScore = memoryPressureScore;
    }

    /// <inheritdoc />
    public override bool Equals(CacheEntryMetadata? other)
    {
        return other is InMemoryCacheEntryMetadata memoryOther &&
               base.Equals(other) &&
               EqualityComparer<long?>.Default.Equals(SizeBytes, memoryOther.SizeBytes) &&
               IsEvictable == memoryOther.IsEvictable &&
               Priority == memoryOther.Priority &&
               MemoryPressureScore == memoryOther.MemoryPressureScore;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;
        hash = hash * 31 + base.GetHashCode();
        hash = hash * 31 + (SizeBytes?.GetHashCode() ?? 0);
        hash = hash * 31 + IsEvictable.GetHashCode();
        hash = hash * 31 + Priority.GetHashCode();
        hash = hash * 31 + MemoryPressureScore.GetHashCode();
        return hash;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var status = IsExpired ? "expired" : "active";
        var sizeInfo = SizeBytes.HasValue ? $"{SizeBytes.Value} bytes" : "unknown size";
        var expiryInfo = ExpiresAtUtc.HasValue ? $"expires {ExpiresAtUtc.Value:yyyy-MM-dd HH:mm:ss} UTC" : "no expiry";
        var evictableInfo = IsEvictable ? "evictable" : "pinned";
        var priorityInfo = Priority.ToString();
        var freshInfo = IsFresh ? "fresh" : "stale";
        
        return $"InMemoryCacheEntry '{Key}' ({status}, {sizeInfo}, {priorityInfo}, {evictableInfo}, {freshInfo}, {expiryInfo})";
    }
}
