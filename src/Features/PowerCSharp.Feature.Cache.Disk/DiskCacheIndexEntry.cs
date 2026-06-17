using System.Text.Json.Serialization;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Index entry for a single cached item. Tracks metadata for eviction and expiry.
/// </summary>
internal sealed class DiskCacheIndexEntry
{
    /// <summary>Relative file path where the cached value is stored.</summary>
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the entry was created.</summary>
    [JsonPropertyName("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>UTC timestamp when the entry was last accessed (for LRU).</summary>
    [JsonPropertyName("lastAccessedUtc")]
    public DateTime LastAccessedUtc { get; set; }

    /// <summary>UTC timestamp when the entry expires (null = no expiry).</summary>
    [JsonPropertyName("expiresAtUtc")]
    public DateTime? ExpiresAtUtc { get; set; }
}
