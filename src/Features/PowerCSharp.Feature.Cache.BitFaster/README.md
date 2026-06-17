# PowerCSharp.Feature.Cache.BitFaster

BitFaster-backed implementation of `PowerCSharp.Feature.Cache`. References `BitFaster.Caching`;
this dependency is isolated here and never enters apps that don't reference this package.

## Usage

```csharp
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

## Isolation

- Don't reference this package → `BitFaster.Caching` is absent from the dependency tree (Layer 1).
- Reference it but disable the flag → the NoOp floor from the contracts package is used (Layer 2).

## Details

- **Package ID:** `PowerCSharp.Feature.Cache.BitFaster`
- **Depends on:** `PowerCSharp.Feature.Cache` + `BitFaster.Caching`
- **Activate:** `PowerFeatures:Cache:Provider = BitFaster`
