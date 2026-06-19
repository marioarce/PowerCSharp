namespace PowerCSharp.Feature.Cache;

/// <summary>Disk-cache settings for the Cache feature.</summary>
public sealed class DiskCacheOptions
{
    /// <summary>Whether the disk cache is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Directory used to store disk-cache entries.</summary>
    public string? DirectoryPath { get; set; }

    /// <summary>Default time-to-live in seconds for disk entries (0 = no expiry).</summary>
    public int DefaultTtlSeconds { get; set; }
}
