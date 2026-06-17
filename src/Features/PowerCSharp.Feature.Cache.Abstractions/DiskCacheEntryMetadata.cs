namespace PowerCSharp.Feature.Cache.Abstractions;

/// <summary>
/// Metadata about a disk cache entry, providing disk-specific diagnostic information.
/// </summary>
public sealed class DiskCacheEntryMetadata : CacheEntryMetadata
{
    /// <summary>
    /// Gets the file kind associated with this cache entry.
    /// </summary>
    public CacheFileKind? FileKind { get; }

    /// <summary>
    /// Gets the file path of the cache entry.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DiskCacheEntryMetadata"/>.
    /// </summary>
    public DiskCacheEntryMetadata(
        string key,
        CacheFileKind? fileKind,
        DateTime createdAtUtc,
        DateTime lastAccessedUtc,
        DateTime? expiresAtUtc = null,
        long? sizeBytes = null,
        string? filePath = null,
        bool isFresh = false,
        long accessCount = 0,
        long hitCount = 0,
        long missCount = 0)
        : base(key, createdAtUtc, lastAccessedUtc, expiresAtUtc, sizeBytes, isFresh, accessCount, hitCount, missCount)
    {
        FileKind = fileKind;
        FilePath = filePath;
    }

    /// <inheritdoc />
    public override bool Equals(CacheEntryMetadata? other)
    {
        return other is DiskCacheEntryMetadata diskOther &&
               base.Equals(other) &&
               EqualityComparer<CacheFileKind?>.Default.Equals(FileKind, diskOther.FileKind) &&
               FilePath == diskOther.FilePath;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;

        hash = hash * 31 + base.GetHashCode();
        hash = hash * 31 + (FileKind?.GetHashCode() ?? 0);
        hash = hash * 31 + (FilePath?.GetHashCode() ?? 0);

        return hash;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var status = IsExpired ? "expired" : "active";
        var sizeInfo = SizeBytes.HasValue ? $"{SizeBytes.Value} bytes" : "unknown size";
        var expiryInfo = ExpiresAtUtc.HasValue ? $"expires {ExpiresAtUtc.Value:yyyy-MM-dd HH:mm:ss} UTC" : "no expiry";
        var fileKindInfo = FileKind != null ? $"{FileKind.Name}" : "unknown kind";
        var freshInfo = IsFresh ? "fresh" : "stale";
        
        return $"DiskCacheEntry '{Key}' ({status}, {sizeInfo}, {fileKindInfo}, {freshInfo}, {expiryInfo})";
    }
}
