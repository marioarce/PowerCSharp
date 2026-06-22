using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Options for the disk-backed cache provider. Bound from configuration via
/// <see cref="CacheDiskExtensions.AddCacheDisk"/>.
/// </summary>
public sealed class DiskCacheFeatureOptions : FeatureOptionsBase
{
    /// <summary>Directory used to store disk-cache entries. Defaults to a subdirectory under <c>Path.GetTempPath()</c>.</summary>
    public string? DirectoryPath { get; set; }

    /// <summary>Default time-to-live in seconds for disk entries (0 = no expiry).</summary>
    public int DefaultTtlSeconds { get; set; }

    /// <summary>Maximum number of entries to store. When exceeded, LRU eviction removes the least-recently-used entries.</summary>
    public int MaxEntries { get; set; } = 10000;

    /// <summary>Whether background cleanup of expired entries is enabled.</summary>
    public bool EnableBackgroundCleanup { get; set; } = true;

    /// <summary>Interval in seconds between background cleanup passes (default: 5 minutes).</summary>
    public int CleanupIntervalSeconds { get; set; } = 300;

    /// <summary>Whether cross-process file locking is enabled. Disabling improves performance in single-process scenarios.</summary>
    public bool EnableCrossProcessLocking { get; set; } = false;
}
