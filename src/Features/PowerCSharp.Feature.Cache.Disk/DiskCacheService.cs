using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerCSharp.Extensions.Objects;
using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Disk-backed LRU cache implementation with atomic writes, file-lock coordination, and background cleanup.
/// Targets netstandard2.0 and net8.0 so it runs on .NET Framework and .NET Core.
/// </summary>
public sealed class DiskCacheService : IDiskCacheService, IDisposable
{
    private const string IndexMutexPrefix = "Global\\PowerCSharp_DiskCache_Index_";
    private const string KeyMutexPrefix = "Global\\PowerCSharp_DiskCache_Key_";
    
    private readonly DiskCacheOptions _options;
    private readonly ILogger<DiskCacheService> _logger;
    private readonly string _rootDirectory;
    private readonly string _indexPath;
    private readonly object _indexLock = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    private DiskCacheIndex _index = new();
    private System.Threading.Timer? _cleanupTimer;
    private readonly ConcurrentDictionary<string, Mutex?> _keyMutexes = new();
    private Mutex? _indexMutex;
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

        if (_options.EnableBackgroundCleanup)
        {
            StartCleanupTimer();
        }

        // Initialize cross-process Mutex if enabled
        if (_options.EnableCrossProcessLocking)
        {
            _indexMutex = CreateIndexMutex();
        }

        _logger.LogInformation("Disk cache initialized at {Directory}", _rootDirectory);
    }

    /// <summary>
    /// Starts the background cleanup timer for all TFMs.
    /// </summary>
    private void StartCleanupTimer()
    {
        var interval = TimeSpan.FromSeconds(_options.CleanupIntervalSeconds);
        _cleanupTimer = new System.Threading.Timer(
            async _ => await PurgeExpiredAsync().ConfigureAwait(false),
            null,
            interval,
            interval);
        _logger.LogDebug("Background cleanup timer started with interval {Interval}s", _options.CleanupIntervalSeconds);
    }

    /// <summary>
    /// Creates the index Mutex for cross-process coordination.
    /// </summary>
    private Mutex CreateIndexMutex()
    {
        var mutexName = $"{IndexMutexPrefix}{_rootDirectory.GetHashCode():X8}";
        
        // Use simple Mutex creation for all TFMs to avoid security complexity
        return new Mutex(false, mutexName);
    }

    /// <summary>
    /// Gets or creates a Mutex for a specific key.
    /// </summary>
    private Mutex GetKeyMutex(string key)
    {
        if (!_options.EnableCrossProcessLocking)
        {
            throw new InvalidOperationException("Cross-process locking is disabled");
        }

        return _keyMutexes.GetOrAdd(key, k =>
        {
            var mutexName = $"{KeyMutexPrefix}{k.GetHashCode():X8}";
            return new Mutex(false, mutexName);
        })!;
    }

    /// <summary>
    /// Executes an action with index Mutex protection.
    /// </summary>
    private void WithIndexLock(Action action)
    {
        if (_options.EnableCrossProcessLocking && _indexMutex != null)
        {
            _indexMutex.WaitOne();
            try
            {
                action();
            }
            finally
            {
                _indexMutex.ReleaseMutex();
            }
        }
        else
        {
            action();
        }
    }

    /// <summary>
    /// Executes an action with key Mutex protection.
    /// </summary>
    private void WithKeyLock(string key, Action action)
    {
        if (_options.EnableCrossProcessLocking)
        {
            var keyMutex = GetKeyMutex(key);
            keyMutex.WaitOne();
            try
            {
                action();
            }
            finally
            {
                keyMutex.ReleaseMutex();
            }
        }
        else
        {
            action();
        }
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
            WithKeyLock(key, () =>
            {
                var fileName = HashFileName(key);
                var filePath = Path.Combine(_rootDirectory, fileName);

                // Atomic write: temp file then move
                var tempPath = filePath + ".tmp";
                using (var stream = File.Create(tempPath))
                {
                    JsonSerializer.SerializeAsync(stream, value, cancellationToken: cancellationToken).GetAwaiter().GetResult();
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
            });
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<T?> GetAsync<T>(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default)
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
    public async ValueTask SetAsync<T>(string key, T value, CacheFileKind fileKind, CancellationToken cancellationToken = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            WithKeyLock(key, () =>
            {
                var fileName = HashFileName(key, fileKind);
                var filePath = Path.Combine(_rootDirectory, fileName);

                // Atomic write: temp file then move
                var tempPath = filePath + ".tmp";
                using (var stream = File.Create(tempPath))
                {
                    JsonSerializer.SerializeAsync(stream, value, cancellationToken: cancellationToken).GetAwaiter().GetResult();
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
            });
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<CacheResult<T>> GetWithMetadataAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!TryGetEntry(key, out var entry))
            {
                return CacheResult<T>.NotFound(key);
            }

            if (entry == null)
            {
                return CacheResult<T>.NotFound(key);
            }

            if (IsExpired(entry))
            {
                var metadata = CreateMetadata(key, entry, isFresh: false);
                RemoveEntry(key);
                return CacheResult<T>.Expired(metadata);
            }

            UpdateLastAccessed(key, entry);
            var filePath = Path.Combine(_rootDirectory, entry.FilePath);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Cache file missing for key {Key}, removing from index", key);
                RemoveEntry(key);
                return CacheResult<T>.Error(key);
            }

            long fileSize = 0;
            T? value = default;

            try
            {
                fileSize = new FileInfo(filePath).Length;
                using (var stream = File.OpenRead(filePath))
                {
                    value = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                if (value != null)
                {
                    var metadata = CreateMetadata(key, entry, fileSize, filePath, isFresh: true);
                    return CacheResult<T>.Success(value, metadata);
                }
                else
                {
                    return CacheResult<T>.Error(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read cache file for key {Key}", key);
                return CacheResult<T>.Error(key);
            }
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<CacheResult<T>> GetWithMetadataAsync<T>(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default)
    {
        // For now, delegate to the standard GetWithMetadataAsync since file kind is already encoded in the file path
        return await GetWithMetadataAsync<T>(key, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!TryGetEntry(key, out var entry))
            {
                return null;
            }

            if (entry == null)
            {
                return null;
            }

            var filePath = Path.Combine(_rootDirectory, entry.FilePath);
            long? fileSize = null;

            if (File.Exists(filePath))
            {
                try
                {
                    fileSize = new FileInfo(filePath).Length;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get file size for key {Key}", key);
                }
            }

            return CreateMetadata(key, entry, fileSize, filePath, isFresh: false);
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default)
    {
        // For now, delegate to the standard GetMetadataAsync since file kind is already encoded in the file path
        return await GetMetadataAsync(key, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates metadata from a cache index entry.
    /// </summary>
    private DiskCacheEntryMetadata CreateMetadata(string key, DiskCacheIndexEntry entry, long? sizeBytes = null, string? filePath = null, bool isFresh = false)
    {
        // Try to determine file kind from file extension
        CacheFileKind? fileKind = null;
        if (filePath != null)
        {
            var extension = Path.GetExtension(filePath);
            var allKinds = CacheFileKind.All;
            fileKind = allKinds.FirstOrDefault(k => k.MatchesExtension(extension));
        }

        return new DiskCacheEntryMetadata(
            key: key,
            fileKind: fileKind,
            createdAtUtc: entry.CreatedAtUtc,
            lastAccessedUtc: entry.LastAccessedUtc,
            expiresAtUtc: entry.ExpiresAtUtc,
            sizeBytes: sizeBytes,
            filePath: filePath,
            isFresh: isFresh);
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

    /// <summary>
    /// Asynchronously purges expired entries from the cache.
    /// </summary>
    public async ValueTask PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() => PurgeExpired(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously evicts least-recently-used entries to stay within the MaxEntries limit.
    /// </summary>
    public async ValueTask EvictToLimitAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() => EvictToLimit(), cancellationToken).ConfigureAwait(false);
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
        WithKeyLock(key, () =>
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
        });
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

    private string HashFileName(string key, CacheFileKind fileKind)
    {
        var hash = key.ComputeHash();
        return hash + fileKind.Extension;
    }

    private void LoadIndex()
    {
        WithIndexLock(() =>
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
        });
    }

    private void SaveIndex()
    {
        WithIndexLock(() =>
        {
            var tempPath = _indexPath + ".tmp";
            var json = JsonSerializer.Serialize(_index, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(tempPath, json);

            if (File.Exists(_indexPath))
            {
                File.Delete(_indexPath);
            }

            File.Move(tempPath, _indexPath);
        });
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cleanupTimer?.Dispose();

        // Dispose cross-process Mutexes
        _indexMutex?.Dispose();
        
        foreach (var keyMutex in _keyMutexes.Values)
        {
            keyMutex?.Dispose();
        }
        _keyMutexes.Clear();

        foreach (var keyLock in _keyLocks.Values)
        {
            keyLock.Dispose();
        }

        _keyLocks.Clear();
    }
}
