# PowerCSharp.Feature.Cache

Cache feature contracts, module, options and NoOp implementations — **no third-party dependencies**.
Pair with a provider package (e.g. `PowerCSharp.Feature.Cache.BitFaster`) to choose a backend.

## Contents

- **`ICacheService` / `IDiskCacheService`** — cache contracts.
- **`CacheFeatureOptions` / `CacheProvider`** — options bound from `PowerFeatures:Cache` (variant `Provider` selects the backend).
- **`CacheFeatureModule`** — auto-discoverable module; registers a NoOp floor so dependents always resolve.
- **NoOp implementations** — safe-off behavior when disabled or no provider is configured.

## Flag

```json
{
  "PowerFeatures": {
    "Cache": { "Enabled": true, "Provider": "BitFaster", "Capacity": 1000 }
  }
}
```

## Details

- **Package ID:** `PowerCSharp.Feature.Cache`
- **Depends on:** `PowerCSharp.Features.Abstractions`
- See: `docs/PowerCSharp.Features.Authoring-Guide.md`
