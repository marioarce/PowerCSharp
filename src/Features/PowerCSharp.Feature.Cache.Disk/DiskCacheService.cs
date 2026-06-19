using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerCSharp.Extensions.Objects;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Helpers;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Disk-backed LRU cache implementation with atomic writes, file-lock coordination, and background cleanup.
/// Targets netstandard2.0 and net8.0 so it runs on .NET Framework and .NET Core.
/// </summary>
public sealed class DiskCacheService : IDiskCacheService, IDisposable
{
    private const string ProviderName = "DiskCache";
    private const string CacheType = "Disk";

    // Mutex prefixes for cross-process synchronization
    private const string IndexMutexPrefix = "Global\\PowerCSharp_DiskCache_Index_";
    private const string KeyMutexPrefix = "Global\\PowerCSharp_DiskCache_Key_";

    // Performance and validation thresholds
    private const int LargeCacheThreshold = 100_000;
    private const int MinimumCleanupIntervalSeconds = 60;
    private const int MaximumDefaultTtlSeconds = 86400; // 24 hours

    // File and path constants
    private const string DefaultCacheDirectoryName = "powercsharp-disk-cache";
    private const string IndexFileName = "index.json";
    private const string TestAccessFileName = ".cache_access_test";

    // Validation character sets
    private static readonly char[] ProblematicPathCharacters = { '<', '>', '"', '|', '*', '?' };

    // Semaphore and concurrency constants
    private const int SemaphoreMaxConcurrency = 1;
    
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
        ValidateOptions(options.Value);
        
        _options = options.Value;
        _logger = logger;

        // Log warnings for potentially problematic configurations
        if (_options.MaxEntries > LargeCacheThreshold)
        {
            _logger.LogWarning(
                "MaxEntries is set to {MaxEntries:N0}, which may impact performance. " +
                "Consider using a smaller value for better memory usage.",
                _options.MaxEntries);
        }

        if (_options.CleanupIntervalSeconds < MinimumCleanupIntervalSeconds && _options.EnableBackgroundCleanup)
        {
            _logger.LogWarning(
                "CleanupIntervalSeconds is set to {CleanupIntervalSeconds} seconds, " +
                "which may impact performance. Consider using a larger interval ({MinimumCleanupIntervalSeconds}+ seconds).",
                _options.CleanupIntervalSeconds,
                MinimumCleanupIntervalSeconds);
        }

        if (_options.DefaultTtlSeconds > MaximumDefaultTtlSeconds) // More than 24 hours
        {
            _logger.LogWarning(
                "DefaultTtlSeconds is set to {DefaultTtlSeconds:N0} seconds ({Days:F1} days), " +
                "which may result in stale cache entries. Consider using a shorter TTL.",
                _options.DefaultTtlSeconds,
                TimeSpan.FromSeconds(_options.DefaultTtlSeconds).TotalDays);
        }

        _rootDirectory = _options.DirectoryPath
            ?? Path.Combine(Path.GetTempPath(), DefaultCacheDirectoryName);

        Directory.CreateDirectory(_rootDirectory);

        _indexPath = Path.Combine(_rootDirectory, IndexFileName);

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
    /// Validates the disk cache options and throws descriptive exceptions for invalid configurations.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="ArgumentException">Thrown when an option has an invalid value.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory path is invalid.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the specified directory is not accessible.</exception>
    private static void ValidateOptions(DiskCacheOptions options)
    {
        // Validate DirectoryPath if specified
        if (!string.IsNullOrEmpty(options.DirectoryPath))
        {
            // Check for invalid path characters
            var invalidChars = Path.GetInvalidPathChars();

            if (options.DirectoryPath.IndexOfAny(invalidChars) >= 0)
            {
                var foundInvalidChars = options
                    .DirectoryPath
                    .Where(c => invalidChars.Contains(c))
                    .Distinct();

                throw new ArgumentException(
                    $"DirectoryPath contains invalid characters: '{options.DirectoryPath}'. " +
                    $"Invalid characters found: {string.Join(", ", foundInvalidChars.Select(c => $"'{c}'"))}",
                    nameof(options.DirectoryPath));
            }

            // Additional validation for common problematic characters that might not be in GetInvalidPathChars
            if (options.DirectoryPath.IndexOfAny(ProblematicPathCharacters) >= 0)
            {
                var foundProblematicChars = options
                    .DirectoryPath
                    .Where(c => ProblematicPathCharacters.Contains(c))
                    .Distinct();

                throw new ArgumentException(
                    $"DirectoryPath contains problematic characters: '{options.DirectoryPath}'. " +
                    $"Problematic characters found: {string.Join(", ", foundProblematicChars.Select(c => $"'{c}'"))}",
                    nameof(options.DirectoryPath));
            }

            // Check if the path is absolute or relative
            var fullPath = Path.GetFullPath(options.DirectoryPath);
            
            // Try to create the directory to validate accessibility
            try
            {
                Directory.CreateDirectory(fullPath);
                
                // Test write access by creating a temporary file
                var testFile = Path.Combine(fullPath, TestAccessFileName);

                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException(
                        $"Directory '{fullPath}' is not writable. " +
                        "Ensure the application has write permissions to the specified directory.");
                }
                catch (IOException ex)
                {
                    throw new IOException(
                        $"Directory '{fullPath}' is not accessible for writing. " +
                        "The path may be on a read-only filesystem or network drive.", ex);
                }
            }
            catch (DirectoryNotFoundException)
            {
                throw new DirectoryNotFoundException(
                    $"Directory path '{options.DirectoryPath}' (resolved to '{fullPath}') does not exist " +
                    "and cannot be created. Ensure the parent directory exists and is accessible.");
            }
            catch (PathTooLongException)
            {
                throw new ArgumentException(
                    $"Directory path '{options.DirectoryPath}' is too long. " +
                    "The path exceeds the maximum allowed length.",
                    nameof(options.DirectoryPath));
            }
        }

        // Validate DefaultTtlSeconds
        if (options.DefaultTtlSeconds < 0)
        {
            throw new ArgumentException(
                $"DefaultTtlSeconds must be non-negative. Value provided: {options.DefaultTtlSeconds}. " +
                "Use 0 for no expiry, or a positive number of seconds for TTL.",
                nameof(options.DefaultTtlSeconds));
        }

        // Validate MaxEntries
        if (options.MaxEntries <= 0)
        {
            throw new ArgumentException(
                $"MaxEntries must be positive. Value provided: {options.MaxEntries}. " +
                "The cache must be able to store at least one entry.",
                nameof(options.MaxEntries));
        }

        // Validate CleanupIntervalSeconds when background cleanup is enabled
        if (options.EnableBackgroundCleanup && options.CleanupIntervalSeconds <= 0)
        {
            throw new ArgumentException(
                $"CleanupIntervalSeconds must be positive when EnableBackgroundCleanup is true. " +
                $"Value provided: {options.CleanupIntervalSeconds}. " +
                "The cleanup interval must be at least 1 second.",
                nameof(options.CleanupIntervalSeconds));
        }
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
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

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
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            WithKeyLock(key, async() =>
            {
                var fileName = HashFileName(key);
                var filePath = Path.Combine(_rootDirectory, fileName);

                // Atomic write: temp file then move
                var tempPath = filePath + ".tmp";

                using (var stream = File.Create(tempPath))
                {
                    await JsonSerializer
                        .SerializeAsync(stream, value, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
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
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

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
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            WithKeyLock(key, async() =>
            {
                var fileName = HashFileName(key, fileKind);
                var filePath = Path.Combine(_rootDirectory, fileName);

                // Atomic write: temp file then move
                var tempPath = filePath + ".tmp";

                using (var stream = File.Create(tempPath))
                {
                    await JsonSerializer
                        .SerializeAsync(stream, value, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
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
    public async ValueTask<CacheResult<T>> GetWithResultAsync<T>(string key, CancellationToken ct = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

        await keyLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            if (!TryGetEntry(key, out var entry))
            {
                return CacheResult<T>.NotFound(key, ProviderName, CacheType, stopwatch.Elapsed);
            }

            if (entry == null)
            {
                return CacheResult<T>.NotFound(key, ProviderName, CacheType, stopwatch.Elapsed);
            }

            if (IsExpired(entry))
            {
                var metadata = CreateMetadata(key, entry, isFresh: false);
                RemoveEntry(key);

                return CacheResult<T>.Expired(metadata, ProviderName, CacheType, stopwatch.Elapsed);
            }

            UpdateLastAccessed(key, entry);

            var filePath = Path.Combine(_rootDirectory, entry.FilePath);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Cache file missing for key {Key}, removing from index", key);
                RemoveEntry(key);

                return CacheResult<T>.Error(key, ProviderName, CacheType, stopwatch.Elapsed);
            }

            long fileSize = 0;
            T? value = default;

            try
            {
                fileSize = new FileInfo(filePath).Length;
                using (var stream = File.OpenRead(filePath))
                {
                    value = await JsonSerializer
                        .DeserializeAsync<T>(stream, cancellationToken: ct)
                        .ConfigureAwait(false);
                }

                if (value != null)
                {
                    var metadata = CreateMetadata(key, entry, fileSize, filePath, isFresh: true);

                    return CacheResult<T>.Success(value, metadata, ProviderName, CacheType, stopwatch.Elapsed);
                }
                else
                {
                    return CacheResult<T>.Error(key, ProviderName, CacheType, stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read cache file for key {Key}", key);

                return CacheResult<T>.Error(key, ProviderName, CacheType, stopwatch.Elapsed);
            }
        }
        finally
        {
            keyLock.Release();
            stopwatch.Stop();
        }
    }

    /// <inheritdoc />
    public async ValueTask<CacheResult<T>> GetWithMetadataAsync<T>(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default)
    {
        // For now, delegate to the standard GetWithResultAsync since file kind is already encoded in the file path
        return await GetWithResultAsync<T>(key, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));
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

    /// <inheritdoc />
    public T? Get<T>(string key, CancellationToken ct = default)
    {
        return AsyncHelper.RunSync(() =>
            GetAsync<T>(key, ct)
        );
    }

    /// <inheritdoc />
    public T? Get<T>(string key, CacheFileKind fileKind, CancellationToken ct = default)
    {
        return AsyncHelper.RunSync(() =>
            GetAsync<T>(key, fileKind, ct)
        );
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, CancellationToken ct = default)
    {
        AsyncHelper.RunSync(() =>
            SetAsync(key, value, ct)
        );
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, CacheFileKind fileKind, CancellationToken ct = default)
    {
        AsyncHelper.RunSync(() =>
            SetAsync(key, value, fileKind, ct)
        );
    }

    /// <inheritdoc />
    public CacheResult<T> GetWithResult<T>(string key, CancellationToken ct = default)
    {
        return AsyncHelper.RunSync(() => GetWithResultAsync<T>(key, ct));
    }

    /// <inheritdoc />
    public void Remove(string key, CancellationToken ct = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

        keyLock.Wait(ct);

        try
        {
            RemoveEntry(key);
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

        await keyLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            RemoveEntry(key);
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public void Clear(CancellationToken ct = default)
    {
        lock (_indexLock)
        {
            var allKeys = _index.Entries.Keys.ToList();

            foreach (var key in allKeys)
            {
                RemoveEntry(key);
            }

            SaveIndex();
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken ct = default)
    {
        await Task.Run(() => Clear(ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetKeys(CancellationToken ct = default)
    {
        lock (_indexLock)
        {
            return _index.Entries.Keys.ToArray();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetKeysAsync(CancellationToken ct = default)
    {
        return await Task.Run(() => GetKeys(ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public CacheEntryMetadata? GetMetadata(string key, CancellationToken ct = default)
    {
        return AsyncHelper.RunSync(() => 
        {
            var valueTask = GetMetadataAsync(key, ct);
            return valueTask.AsTask();
        });
    }

    /// <inheritdoc />
    async Task<CacheEntryMetadata?> ICacheStore.GetMetadataAsync(string key, CancellationToken ct)
    {
        return await GetMetadataAsync(key, ct)
            .AsTask()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<CacheResult<T>> GetWithResultAsync<T>(string key, CacheFileKind fileKind, CancellationToken ct = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));
        await keyLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            if (!TryGetEntry(key, out var entry))
            {
                return CacheResult<T>.NotFound(key, ProviderName, CacheType, stopwatch.Elapsed);
            }

            if (entry == null)
            {
                return CacheResult<T>.NotFound(key, ProviderName, CacheType, stopwatch.Elapsed);
            }

            if (IsExpired(entry))
            {
                var metadata = CreateMetadata(key, entry, isFresh: false);

                RemoveEntry(key);

                return CacheResult<T>.Expired(metadata, ProviderName, CacheType, stopwatch.Elapsed);
            }

            UpdateLastAccessed(key, entry);
            var filePath = Path.Combine(_rootDirectory, entry.FilePath);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Cache file missing for key {Key}, removing from index", key);
                RemoveEntry(key);
                return CacheResult<T>.Error(key, ProviderName, CacheType, stopwatch.Elapsed);
            }

            long fileSize = 0;
            T? value = default;

            try
            {
                fileSize = new FileInfo(filePath).Length;
                using (var stream = File.OpenRead(filePath))
                {
                    value = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: ct).ConfigureAwait(false);
                }

                if (value != null)
                {
                    var metadata = CreateMetadata(key, entry, fileSize, filePath, isFresh: true);
                    return CacheResult<T>.Success(value, metadata, ProviderName, CacheType, stopwatch.Elapsed);
                }
                else
                {
                    return CacheResult<T>.Error(key, ProviderName, CacheType, stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read cache file for key {Key}", key);
                return CacheResult<T>.Error(key, ProviderName, CacheType, stopwatch.Elapsed);
            }
        }
        finally
        {
            keyLock.Release();
            stopwatch.Stop();
        }
    }

    /// <inheritdoc />
    public async ValueTask<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

        await keyLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            // Try to get existing value
            if (TryGetEntry(key, out var entry) && entry != null && !IsExpired(entry))
            {
                UpdateLastAccessed(key, entry);
                var filePath = Path.Combine(_rootDirectory, entry.FilePath);

                if (File.Exists(filePath))
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var value = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: ct).ConfigureAwait(false);
                        return value!;
                    }
                }
                else
                {
                    _logger.LogWarning("Cache file missing for key {Key}, removing from index", key);
                    RemoveEntry(key);
                }
            }

            // Create new value
            var created = await factory().ConfigureAwait(false);
            await SetAsync(key, created, ct).ConfigureAwait(false);

            return created;
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<T> GetOrCreateAsync<T>(string key, CacheFileKind fileKind, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(SemaphoreMaxConcurrency, SemaphoreMaxConcurrency));

        await keyLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            // Try to get existing value
            if (TryGetEntry(key, out var entry) && entry != null && !IsExpired(entry))
            {
                UpdateLastAccessed(key, entry);
                var filePath = Path.Combine(_rootDirectory, entry.FilePath);

                if (File.Exists(filePath))
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var value = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: ct).ConfigureAwait(false);
                        return value!;
                    }
                }
                else
                {
                    _logger.LogWarning("Cache file missing for key {Key}, removing from index", key);
                    RemoveEntry(key);
                }
            }

            // Create new value
            var created = await factory().ConfigureAwait(false);
            await SetAsync(key, created, fileKind, ct).ConfigureAwait(false);

            return created;
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        return AsyncHelper.RunSync(() => GetOrCreateAsync(key, () => Task.Run(factory, ct), ttl, ct));
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

    /// <summary>
    /// Releases the resources used by the disk cache service, including the cleanup timer
    /// and cross-process mutexes.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

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
