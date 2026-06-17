using System.Text.Json.Serialization;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// In-memory representation of the disk-cache index. Serialized to a single JSON file
/// for fast lookups and LRU tracking. Updated atomically on each write.
/// </summary>
internal sealed class DiskCacheIndex
{
    /// <summary>Map of cache keys to index entries (metadata + file path).</summary>
    [JsonPropertyName("entries")]
    public Dictionary<string, DiskCacheIndexEntry> Entries { get; set; } = new();
}
