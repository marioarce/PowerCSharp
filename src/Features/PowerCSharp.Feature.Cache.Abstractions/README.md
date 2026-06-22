# PowerCSharp.Feature.Cache.Abstractions

Framework-agnostic contracts and safe-off NoOp implementations for the PowerCSharp Cache feature.

- Targets `netstandard2.0` and `net8.0`, so cache providers can run on **.NET Framework** and **.NET Core**.
- No ASP.NET Core dependency.
- Only dependency is `Microsoft.Extensions.Logging.Abstractions` (required for the `ILogger` parameter on NoOp services).

## Contents

- `ICacheService` — in-memory cache abstraction (with `GetOrCreate` stampede-safe helpers).
- `ICacheStore` — low-level cache store abstraction.
- `IDiskCacheService` — asynchronous disk-backed cache abstraction.
- `CacheProvider` — backend selector enum.
- `CacheResult<T>` / `CacheResultReason` — operation result model.
- `CacheEntryMetadata`, `InMemoryCacheEntryMetadata`, `DiskCacheEntryMetadata`, `CacheFileKind` — metadata types.
- `NoOpCacheService` / `NoOpDiskCacheService` — safe-off floors so dependents always resolve.

## Namespaces

This package uses three namespaces; import the ones you need:

```csharp
using PowerCSharp.Feature.Cache.Abstractions;        // ICacheService, IDiskCacheService, CacheResult<T>, metadata
using PowerCSharp.Feature.Cache.Abstractions.Enums;  // CacheProvider, CacheResultReason, CacheEntryPriority
using PowerCSharp.Feature.Cache.Abstractions.NoOp;   // NoOpCacheService, NoOpDiskCacheService
```

> `CacheProvider` is **not** in the root `PowerCSharp.Feature.Cache.Abstractions` namespace; it is in the `.Enums` sub-namespace.

## Providers

- `PowerCSharp.Feature.Cache.BitFaster` — in-memory provider.
- `PowerCSharp.Feature.Cache.Disk` — disk-backed LRU provider.

The ASP.NET feature-module wiring lives in `PowerCSharp.Feature.Cache`.

## Details

- **Package ID:** `PowerCSharp.Feature.Cache.Abstractions`
- **Depends on:** `Microsoft.Extensions.Logging.Abstractions`
- **Target frameworks:** `netstandard2.0` and `net8.0`
