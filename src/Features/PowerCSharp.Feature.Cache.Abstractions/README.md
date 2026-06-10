# PowerCSharp.Feature.Cache.Abstractions

Framework-agnostic contracts for the PowerCSharp Cache feature.

- Targets `netstandard2.0` and `net8.0`, so cache providers can run on **.NET Framework** and **.NET Core**.
- No ASP.NET Core dependency.

## Contents

- `ICacheService` — in-memory cache abstraction (with `GetOrCreate` stampede-safe helpers).
- `IDiskCacheService` — asynchronous disk-backed cache abstraction.
- `CacheProvider` — backend selector enum.
- NoOp implementations (`NoOpCacheService`, `NoOpDiskCacheService`) — safe-off floors so dependents always resolve.

## Providers

- `PowerCSharp.Feature.Cache.BitFaster` — in-memory provider.
- `PowerCSharp.Feature.Cache.Disk` — disk-backed LRU provider.

The ASP.NET feature-module wiring lives in `PowerCSharp.Feature.Cache`.
