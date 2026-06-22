# PowerCSharp.Feature.Cache — API Reference

> The Cache pluggable feature family: framework-agnostic contracts, the feature module, and two provider implementations. Each package in the family versions independently under `PowerCSharpFeatureCacheVersion`.

---

## Package Family Overview

| Package | Role | Target Frameworks | Version |
|---|---|---|---|
| `PowerCSharp.Feature.Cache.Abstractions` | Contracts + NoOp floor | `netstandard2.0` + `net8.0` | `$(PowerCSharpFeatureCacheVersion)` |
| `PowerCSharp.Feature.Cache` | Module + options + ASP.NET Core wiring | `net8.0` | `$(PowerCSharpFeatureCacheVersion)` |
| `PowerCSharp.Feature.Cache.BitFaster` | BitFaster in-memory LRU provider | `netstandard2.0` + `net8.0` | `$(PowerCSharpFeatureCacheVersion)` |
| `PowerCSharp.Feature.Cache.Disk` | Disk-backed LRU provider | `net8.0` | `$(PowerCSharpFeatureCacheVersion)` |

### Dependency direction

```
PowerCSharp.Feature.Cache.Abstractions   (contracts + NoOp, no third-party deps)
          ▲                    ▲
          │                    │
PowerCSharp.Feature.Cache      │         (module + options → depends on engine abstractions too)
          ▲                    │
          │                    │
PowerCSharp.Feature.Cache.BitFaster    PowerCSharp.Feature.Cache.Disk
   (isolates BitFaster.Caching)           (no third-party deps)
```

---

## 1. PowerCSharp.Feature.Cache.Abstractions

### Core interfaces

#### `ICacheService`

```csharp
public interface ICacheService
{
    // Sync
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    bool Remove(string key);
    void Clear();
    IEnumerable<string> GetKeys();

    // Async
    ValueTask<CacheResult<T>> GetAsync<T>(string key, CancellationToken ct = default);
    ValueTask SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
    ValueTask<bool> RemoveAsync(string key, CancellationToken ct = default);
    ValueTask ClearAsync(CancellationToken ct = default);

    // Stampede protection
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null);
    ValueTask<T> GetOrCreateAsync<T>(string key, Func<ValueTask<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default);

    // Metadata
    CacheMetadata? GetMetadata(string key);
}
```

#### `IDiskCacheService`

Extends `ICacheService` with disk-specific members:

```csharp
public interface IDiskCacheService : ICacheService
{
    string CacheDirectory { get; }
    long ApproximateSizeBytes { get; }

    ValueTask CleanupAsync(CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default);
    ValueTask<CacheMetadata?> GetMetadataAsync(string key, CancellationToken ct = default);
}
```

#### `ICacheStore`

Low-level store abstraction used internally by providers. Not intended for direct consumption.

### Value types

#### `CacheResult<T>`

```csharp
public readonly struct CacheResult<T>
{
    public bool Hit { get; }
    public T? Value { get; }
    public CacheMetadata? Metadata { get; }

    public static CacheResult<T> Miss();
    public static CacheResult<T> Found(T value, CacheMetadata? metadata = null);
}
```

#### `CacheMetadata`

```csharp
public sealed class CacheMetadata
{
    public string Key { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public long? SizeBytes { get; init; }
    public CacheProvider Provider { get; init; }
    public long HitCount { get; set; }
}
```

#### `CacheFileKind` (disk-specific)

```csharp
public enum CacheFileKind
{
    Data,       // main cache entry file
    Lock,       // cross-process file lock
    Metadata    // sidecar metadata file
}
```

#### `CacheProvider` enum

```csharp
public enum CacheProvider
{
    None,
    NoOp,
    BitFaster,
    Disk,
    Memory      // future
}
```

### NoOp implementations

| Class | Interface | Behaviour |
|---|---|---|
| `NoOpCacheService` | `ICacheService` | All reads return miss; writes are no-ops. Safe-off floor when no provider is active. |
| `NoOpDiskCacheService` | `IDiskCacheService` | Extends `NoOpCacheService`; disk-specific members return empty/zero. |

---

## 2. PowerCSharp.Feature.Cache

### `CacheFeatureModule`

`IFeatureModule` implementation. Auto-discoverable (parameterless constructor). Always registered via `ConfigureServices` (Model A — module self-gates).

```csharp
public const string Key = "Cache";   // FeatureKey
public int Order => 100;
```

`ConfigureServices` behaviour:
1. Binds `CacheFeatureOptions` from `PowerFeatures:Cache` config section.
2. Registers `NoOpCacheService` and `NoOpDiskCacheService` via `TryAddSingleton` (safe-off floor).
3. Logs an info message when the feature is disabled.
4. Provider packages use plain `AddSingleton` which takes precedence over the `TryAdd` NoOp.

### `CacheFeatureOptions` (inherits `FeatureOptionsBase`)

```csharp
public sealed class CacheFeatureOptions : FeatureOptionsBase
{
    public bool Enabled { get; set; }        // inherited
    public CacheProvider Provider { get; set; }
    public int Capacity { get; set; } = 1000;
    public TimeSpan? DefaultTtl { get; set; }
}
```

### `AddCacheFeature` (explicit extension)

```csharp
IServiceCollection AddCacheFeature(
    this IServiceCollection services,
    IConfiguration configuration)
```

Explicit alternative to auto-discovery. Calls `ConfigureServices` directly without going through the engine scan.

### Configuration

```json
{
  "PowerFeatures": {
    "Cache": {
      "Enabled": true,
      "Provider": "BitFaster",
      "Capacity": 5000,
      "DefaultTtl": "00:05:00"
    }
  }
}
```

---

## 3. PowerCSharp.Feature.Cache.BitFaster

### `BitFasterCacheService`

Implements `ICacheService` backed by `BitFaster.Caching.ConcurrentLru<string, CacheEntry<T>>`. The `BitFaster.Caching` dependency is isolated to this package.

| Feature | Detail |
|---|---|
| **Target** | `netstandard2.0` + `net8.0` |
| **Eviction** | Concurrent LRU (W-TinyLFU variant via BitFaster) |
| **Stampede protection** | `GetOrCreate` / `GetOrCreateAsync` with `GetOrAdd` atomicity |
| **Metadata** | Hit count, created/expires timestamps, provider tag |
| **Thread safety** | Fully thread-safe (lock-free via BitFaster internals) |

### Registration

Provider packages register via plain `AddSingleton` (takes precedence over the NoOp `TryAdd`):

```csharp
services.AddSingleton<ICacheService, BitFasterCacheService>();
```

This is wired automatically when `CacheFeatureModule` runs and the `Provider` option resolves to `BitFaster`.

---

## 4. PowerCSharp.Feature.Cache.Disk

### `DiskCacheService`

Implements `IDiskCacheService` with a local-disk LRU store.

| Feature | Detail |
|---|---|
| **Target** | `net8.0` |
| **Storage** | JSON-serialized entries under `DiskCacheFeatureOptions.CacheDirectory` |
| **Eviction** | LRU eviction when `MaxEntries` is exceeded |
| **Atomic writes** | Write-to-temp + atomic rename prevents partial reads |
| **Cross-process locking** | Per-key `.lock` files using `FileStream` with `FileShare.None` |
| **Background cleanup** | `DiskCacheBackgroundService` (`IHostedService`) runs periodic cleanup |
| **Metadata** | Sidecar `.meta` files store `CacheMetadata` alongside data files |
| **Debugging** | `DebugHelper` provides `DumpState()` for diagnostics |

### `DiskCacheFeatureOptions`

```csharp
public sealed class DiskCacheFeatureOptions
{
    public string CacheDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "PowerCSharp.Cache");
    public int MaxEntries { get; set; } = 1000;
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(15);
}
```

### Configuration

```json
{
  "PowerFeatures": {
    "Cache": {
      "Enabled": true,
      "Provider": "Disk"
    },
    "Cache:Disk": {
      "CacheDirectory": "/var/cache/myapp",
      "MaxEntries": 5000,
      "DefaultTtl": "02:00:00",
      "CleanupInterval": "00:30:00"
    }
  }
}
```

### `CacheFileKind` file layout

```
<CacheDirectory>/
  <key-hash>.dat       CacheFileKind.Data     — serialised value
  <key-hash>.meta      CacheFileKind.Metadata — CacheMetadata JSON
  <key-hash>.lock      CacheFileKind.Lock     — cross-process lock (transient)
```

---

## 5. Two-Layer Gating

| | No package ref | Package ref + flag off | Package ref + flag on |
|---|---|---|---|
| **Result** | Absent — no code, no deps | NoOp registered (safe-off) | Active provider registered |

---

## 6. Host Integration — Full Example

```csharp
// Program.cs

// Option A: auto-discovery via engine scan
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.ScanAssemblies(typeof(CacheFeatureModule).Assembly);
});

// Option B: explicit registration (no reflection)
builder.Services.AddCacheFeature(builder.Configuration);

var app = builder.Build();
app.UsePowerFeatures();
```

```csharp
// In a service/controller
public class MyService(ICacheService cache)
{
    public async Task<MyData> GetDataAsync(string key)
    {
        var result = await cache.GetAsync<MyData>(key);
        if (result.Hit)
            return result.Value!;

        var data = await FetchFromSourceAsync(key);
        await cache.SetAsync(key, data, TimeSpan.FromMinutes(10));
        return data;
    }
}
```

---

## 7. Related Documents

- [`PowerCSharp.Features.Architecture.md`](PowerCSharp.Features.Architecture.md) — Two-tier design, dependency topology
- [`PowerCSharp.Features.Authoring-Guide.md`](PowerCSharp.Features.Authoring-Guide.md) — Cache family as the canonical worked example
- [`PowerCSharp.Feature.Cache.Disk.Plan.md`](PowerCSharp.Feature.Cache.Disk.Plan.md) — Disk cache design notes and implementation history
- [`PowerCSharp.Features.md`](PowerCSharp.Features.md) — Features engine API reference
