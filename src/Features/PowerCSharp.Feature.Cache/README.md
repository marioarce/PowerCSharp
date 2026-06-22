# PowerCSharp.Feature.Cache

Cache feature module, options, and ASP.NET Core wiring — **no third-party dependencies**.

Pair this package with `PowerCSharp.Feature.Cache.Abstractions` (contracts + NoOp) and a provider package (e.g. `PowerCSharp.Feature.Cache.BitFaster`) to choose a backend.

## Contents

- **`CacheFeatureOptions`** — options bound from `PowerFeatures:Cache` (variant `Provider` selects the backend).
- **`CacheFeatureModule`** — auto-discoverable module; registers a NoOp floor so dependents always resolve.
- **`AddCacheFeature`** — explicit registration extension.

## Namespaces

- `PowerCSharp.Feature.Cache` — options, module, and extension methods.

> The contracts (`ICacheService`, `IDiskCacheService`) and the NoOp implementations live in `PowerCSharp.Feature.Cache.Abstractions`.

## Usage

```csharp
using PowerCSharp.Feature.Cache;

builder.Services
    .AddPowerFeatures(builder.Configuration, o => o.ScanAssemblies(typeof(CacheFeatureModule).Assembly))
    .AddCacheBitFaster(builder.Configuration);
```

```json
{
  "PowerFeatures": {
    "Cache": { "Enabled": true, "Provider": "BitFaster", "Capacity": 1000 }
  }
}
```

## Details

- **Package ID:** `PowerCSharp.Feature.Cache`
- **Depends on:** `PowerCSharp.Features.Abstractions` + `PowerCSharp.Feature.Cache.Abstractions`
- **Target framework:** `net8.0` (requires ASP.NET Core host)
- See: `docs/PowerCSharp.Features.Authoring-Guide.md`
