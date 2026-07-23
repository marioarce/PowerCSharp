# PowerCSharp.Feature.Cache.BitFaster

![PowerCSharp Banner](https://raw.githubusercontent.com/marioarce/PowerCSharp/0191ee12092c28ccf5a578e59977583117a3ff00/docs/images/PowerCSharp_Banner.png)

BitFaster-backed implementation of the PowerCSharp Cache feature. References `BitFaster.Caching`;
this dependency is isolated here and never enters apps that don't reference this package.

## Usage

```csharp
using PowerCSharp.Feature.Cache;
using PowerCSharp.Feature.Cache.BitFaster;

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
- Reference it but disable the flag → the NoOp floor from `PowerCSharp.Feature.Cache.Abstractions` is used (Layer 2).

## Provider-only usage

You can use this provider without the ASP.NET Core module:

```csharp
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.BitFaster;

services.AddCacheBitFaster(configuration);
```

## Details

- **Package ID:** `PowerCSharp.Feature.Cache.BitFaster`
- **Depends on:** `PowerCSharp.Feature.Cache.Abstractions` + `BitFaster.Caching`
- **Activate:** `PowerFeatures:Cache:Provider = BitFaster` or call `AddCacheBitFaster(...)`
