using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerCSharp.Extensions.Objects;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Disk-backed LRU cache implementation with atomic writes, file-lock coordination, and background cleanup.
/// Targets netstandard2.0 and net8.0 so it runs on .NET Framework and .NET Core.
/// </summary>
public sealed class DiskCacheService : IDiskCacheService, IDisposable
{
    private readonly DiskCacheOptions _options;
    private readonly ILogger<DiskCacheService> _logger;
    private readonly string _rootDirectory;
    private readonly string _indexPath;
    private readonly object _indexLock = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    private DiskCacheIndex _index = new();
    private bool _disposed;

    /// <summary>Creates the disk cache, ensuring the storage directory exists.</summary>
    public DiskCacheService(IOptions<DiskCacheOptions> options, ILogger<DiskCacheService> logger)
    {
        _options = options.Value;
        _logger = logger;

        _rootDirectory = _options.DirectoryPath
            ?? Path.Combine(Path.GetTempPath(), "powercsharp-disk-cache");
        Directory.CreateDirectory(_rootDirectory);

        _indexPath = Path.Combine(_rootDirectory, "index.json");
        LoadIndex();

        _logger.LogInformation("Disk cache initialized at {Directory}", _rootDirectory);
    }

    /// <inheritdoc />
    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!TryGetEntry(key, out var entry))
            {
                return default;
            }

            if (entry == null)
            {
                return default;
            }

            if (IsExpired(entry))
            {
                RemoveEntry(key);
                return default;
            }

            UpdateLastAccessed(key, entry);
            var filePath = Path.Combine(_rootDirectory, entry.FilePath);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Cache file missing for key {Key}, removing from index", key);
                RemoveEntry(key);
                return default;
            }

            using (var stream = File.OpenRead(filePath))
            {
                var value = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
                return value;
            }
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var fileName = HashFileName(key);
            var filePath = Path.Combine(_rootDirectory, fileName);

            // Atomic write: temp file then move
            var tempPath = filePath + ".tmp";
            using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, value, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Move(tempPath, filePath);

            // Update index
            var now = DateTime.UtcNow;
            var entry = new DiskCacheIndexEntry
            {
                FilePath = fileName,
                CreatedAtUtc = now,
                LastAccessedUtc = now,
                ExpiresAtUtc = _options.DefaultTtlSeconds > 0 ? now.AddSeconds(_options.DefaultTtlSeconds) : null
            };

            lock (_indexLock)
            {
                _index.Entries[key] = entry;
                EvictIfNeeded();
                SaveIndex();
            }
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <summary>
    /// Purges expired entries from the cache. Synchronous primitive that can be called manually
    /// or by the background timer.
    /// </summary>
    public void PurgeExpired()
    {
        lock (_indexLock)
        {
            var expiredKeys = _index.Entries
                .Where(kvp => IsExpired(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                RemoveEntry(key);
            }

            if (expiredKeys.Count > 0)
            {
                SaveIndex();
                _logger.LogInformation("Purged {Count} expired entries", expiredKeys.Count);
            }
        }
    }

    /// <summary>
    /// Evicts least-recently-used entries to stay within the MaxEntries limit.
    /// </summary>
    public void EvictToLimit()
    {
        lock (_indexLock)
        {
            if (_index.Entries.Count <= _options.MaxEntries)
            {
                return;
            }

            var toEvict = _index.Entries
                .OrderBy(kvp => kvp.Value.LastAccessedUtc)
                .Take(_index.Entries.Count - _options.MaxEntries)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in toEvict)
            {
                RemoveEntry(key);
            }

            SaveIndex();
            _logger.LogInformation("Evicted {Count} LRU entries", toEvict.Count);
        }
    }

    private bool TryGetEntry(string key, out DiskCacheIndexEntry? entry)
    {
        lock (_indexLock)
        {
            return _index.Entries.TryGetValue(key, out entry);
        }
    }

    private void UpdateLastAccessed(string key, DiskCacheIndexEntry? entry)
    {
        if (entry == null)
        {
            return;
        }

        lock (_indexLock)
        {
            entry.LastAccessedUtc = DateTime.UtcNow;
            SaveIndex();
        }
    }

    private void RemoveEntry(string key)
    {
        lock (_indexLock)
        {
            if (_index.Entries.TryGetValue(key, out var entry))
            {
                var filePath = Path.Combine(_rootDirectory, entry.FilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _index.Entries.Remove(key);
            }
        }
    }

    private void EvictIfNeeded()
    {
        if (_index.Entries.Count > _options.MaxEntries)
        {
            var toEvict = _index.Entries
                .OrderBy(kvp => kvp.Value.LastAccessedUtc)
                .Take(_index.Entries.Count - _options.MaxEntries)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in toEvict)
            {
                RemoveEntry(key);
            }
        }
    }

    private bool IsExpired(DiskCacheIndexEntry? entry)
    {
        if (entry == null)
        {
            return false;
        }

        return entry.ExpiresAtUtc.HasValue && DateTime.UtcNow > entry.ExpiresAtUtc.Value;
    }

    private string HashFileName(string key)
    {
        var hash = key.ComputeHash();
        return hash + ".json";
    }

    private void LoadIndex()
    {
        if (File.Exists(_indexPath))
        {
            try
            {
                var json = File.ReadAllText(_indexPath);
                _index = JsonSerializer.Deserialize<DiskCacheIndex>(json) ?? new DiskCacheIndex();
                _logger.LogInformation("Loaded index with {Count} entries", _index.Entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load index, starting fresh");
                _index = new DiskCacheIndex();
            }
        }
    }

    private void SaveIndex()
    {
        var tempPath = _indexPath + ".tmp";
        var json = JsonSerializer.Serialize(_index, new JsonSerializerOptions { WriteIndented = false });
        File.WriteAllText(tempPath, json);

        if (File.Exists(_indexPath))
        {
            File.Delete(_indexPath);
        }

        File.Move(tempPath, _indexPath);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var keyLock in _keyLocks.Values)
        {
            keyLock.Dispose();
        }

        _keyLocks.Clear();
    }
}
