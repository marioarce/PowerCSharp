# PowerCSharp.Feature.Cache.Disk

Disk-backed LRU cache implementation for the PowerCSharp Cache feature.

- Targets `net8.0`, so it runs on **.NET 8.0 and later**.
- Atomic writes (temp file + `File.Move`) so readers never see partial files.
- LRU eviction with configurable count cap and TTL expiry.
- Background cleanup via portable timer (all TFMs) + `IHostedService` on net8.0 for graceful shutdown.
- Single JSON index file for fast lookups.
- Cross-process file-lock coordination (configurable).

## Usage

```csharp
services.AddCacheDisk(configuration);
```

Configuration section:

```json
{
  "PowerFeatures": {
    "DiskCache": {
      "DirectoryPath": "/path/to/cache",
      "DefaultTtlSeconds": 3600,
      "MaxEntries": 10000,
      "EnableBackgroundCleanup": true,
      "CleanupIntervalSeconds": 300,
      "EnableCrossProcessLocking": false
    }
  }
}
```

## Manual control

```csharp
var disk = serviceProvider.GetRequiredService<IDiskCacheService>();
await disk.SetAsync("key", value);
var result = await disk.GetAsync<MyType>("key");
```

For manual eviction/expiry:

```csharp
var cache = (DiskCacheService)disk;
cache.PurgeExpired();
cache.EvictToLimit();
```

## Details

- **Package ID:** `PowerCSharp.Feature.Cache.Disk`
- **Depends on:** `PowerCSharp.Feature.Cache.Abstractions` (+ `PowerCSharp.Core`, `PowerCSharp.Helpers`, `PowerCSharp.Extensions`)
- **Target framework:** `net8.0`
- **Activate:** `PowerFeatures:DiskCache:Enabled = true` or call `services.AddCacheDisk(configuration)`

## Namespaces

- `PowerCSharp.Feature.Cache.Disk` — `DiskCacheService`, `DiskCacheFeatureOptions`, `AddCacheDisk`, `DiskCacheBackgroundService`.
- `PowerCSharp.Feature.Cache.Abstractions` — `IDiskCacheService` contract.
