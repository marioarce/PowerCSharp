# PowerCSharp.Feature.Cache.Disk

Disk-backed LRU cache implementation for the PowerCSharp Cache feature.

- Targets `netstandard2.0` and `net8.0`, so it runs on **.NET Framework** and **.NET Core**.
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
    "Cache": {
      "Disk": {
        "DirectoryPath": "/path/to/cache",
        "DefaultTtlSeconds": 3600,
        "MaxEntries": 1000,
        "EnableBackgroundCleanup": true,
        "CleanupIntervalSeconds": 300,
        "EnableCrossProcessLocking": true
      }
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
