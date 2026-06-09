# PowerCSharp.Features

The Features engine: feature discovery (hybrid auto-scan + explicit), composite flag resolution,
DI orchestration, a feature registry, and diagnostics.

## Host integration

```csharp
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.ScanAssemblies(typeof(CacheFeatureModule).Assembly); // opt-in auto-discovery
    options.Override("Cache", true);                              // optional explicit override
    options.EnableDiagnosticsEndpoint();                          // opt-in, off by default
});

var app = builder.Build();
app.UsePowerFeatures(); // applies enabled features' middleware in Order
```

## Flag resolution (highest precedence first)

1. Explicit code override (`options.Override`)
2. Custom `IFeatureFlagProvider` (`options.AddFlagProvider`)
3. Environment variables (`POWERFEATURES__<KEY>__ENABLED`)
4. `appsettings` (`PowerFeatures:<Key>:Enabled`)
5. Feature default

## Details

- **Package ID:** `PowerCSharp.Features`
- **Depends on:** `PowerCSharp.Features.Abstractions`
- See: `docs/PowerCSharp.Features.Architecture.md`, `docs/PowerCSharp.Features.FlagReference.md`
