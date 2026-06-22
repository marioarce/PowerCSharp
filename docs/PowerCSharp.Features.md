# PowerCSharp.Features — API Reference

> Engine package for the PowerCSharp Features system. Provides module discovery, composite flag resolution, DI orchestration, the feature registry, and diagnostics. Read [`PowerCSharp.Features.Architecture.md`](PowerCSharp.Features.Architecture.md) for the conceptual model and [`PowerCSharp.Features.Authoring-Guide.md`](PowerCSharp.Features.Authoring-Guide.md) to build a new feature.

---

## Package Information

| Property | Value |
|---|---|
| **Package ID** | `PowerCSharp.Features` |
| **Version** | `$(PowerCSharpFeaturesVersion)` |
| **Target Framework** | `net8.0` |
| **Dependencies** | `PowerCSharp.Features.Abstractions`, `Microsoft.AspNetCore.App` |
| **NuGet** | [PowerCSharp.Features](https://www.nuget.org/packages/PowerCSharp.Features) |

---

## 1. Host Integration

### `AddPowerFeatures` (IServiceCollection extension)

```csharp
IServiceCollection AddPowerFeatures(
    this IServiceCollection services,
    IConfiguration configuration,
    Action<PowerFeaturesOptions>? configure = null)
```

Registers the composite flag resolver, discovers `IFeatureModule` implementations from opted-in assemblies, invokes each module's `ConfigureServices` (modules self-gate on their flag), and records a `FeatureRegistry` for diagnostics.

**Example:**

```csharp
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.AddBuiltInFeatures();
    options.ScanAssemblies(typeof(CacheFeatureModule).Assembly);
    options.Override("Cache", true);
    options.EnableDiagnosticsEndpoint();
});
```

### `UsePowerFeatures` (IApplicationBuilder extension)

```csharp
IApplicationBuilder UsePowerFeatures(this IApplicationBuilder app)
```

Logs the resolved feature matrix via `ILogger` and invokes `ConfigurePipeline` for each enabled, middleware-bearing feature in ascending `Order`. Optionally maps the diagnostics endpoint.

```csharp
var app = builder.Build();
app.UsePowerFeatures();
```

---

## 2. `PowerFeaturesOptions`

Fluent configuration object supplied via the `AddPowerFeatures` callback. All methods return `this` for chaining.

| Method | Description |
|---|---|
| `ScanAssemblies(params Assembly[])` | Opts the supplied assemblies into auto-discovery of `IFeatureModule` implementations. |
| `AddModule(IFeatureModule)` | Registers an explicit module instance — no reflection required. |
| `Override(string featureKey, bool enabled)` | Sets a boolean code-level override (highest flag precedence). |
| `Override(string featureKey, string value)` | Sets a variant/value code-level override. |
| `AddFlagProvider(IFeatureFlagProvider)` | Adds a custom flag provider (e.g. Azure App Config, secret-driven). |
| `EnableDiagnosticsEndpoint(string? path = null)` | Enables the opt-in `GET /power-features` endpoint (default path). |

---

## 3. Flag Resolution

The engine composes a **composite resolver** from all registered providers in this precedence order (highest first):

1. **Explicit code override** — `options.Override(...)`
2. **Custom `IFeatureFlagProvider`** — `options.AddFlagProvider(...)`
3. **Environment variables** — `POWERFEATURES__<KEY>__ENABLED` (and `__<PROPERTY>` for variants)
4. **`appsettings`** — `PowerFeatures:<Key>:Enabled` (plus variant properties)
5. **Feature default** — `FeatureDescriptor.DefaultEnabled` (currently `false`)

### Configuration shape

```json
{
  "PowerFeatures": {
    "Cache":    { "Enabled": true, "Provider": "BitFaster", "Capacity": 1000 },
    "Cors":     { "Enabled": true, "AllowedOrigins": [ "https://example.com" ] },
    "Sitecore": { "Enabled": false }
  }
}
```

### Environment variable convention

```
POWERFEATURES__CACHE__ENABLED=true
POWERFEATURES__CACHE__PROVIDER=BitFaster
```

---

## 4. Built-in Flag Providers

### `ConfigurationFeatureFlagProvider`

Reads `PowerFeatures:<Key>:Enabled` (and variant properties) from `IConfiguration`.

### `EnvironmentFeatureFlagProvider`

Reads `POWERFEATURES__<KEY>__ENABLED` (and `__<PROPERTY>`) from environment variables. Uses double-underscore (`__`) as the section separator per .NET conventions.

### `OverrideFeatureFlagProvider`

In-memory store for code-level overrides set via `options.Override(...)`. Always highest precedence.

### `CompositeFeatureFlagProvider`

Chains all registered providers. Returns the first provider that has a value for the requested key. Implements `IFeatureFlagProvider`.

---

## 5. Discovery

### `FeatureModuleDiscovery` (internal)

Merges explicit modules with reflection-based discovery from opted-in assemblies. De-duplicates by concrete type. Orders by `IFeatureModule.Order` ascending, then `FeatureKey` alphabetically.

Discovery is **opt-in per assembly** — no assembly is scanned unless explicitly passed to `ScanAssemblies(...)`. Nothing is registered by surprise.

---

## 6. `FeatureRegistry`

Registered as a singleton in DI. Holds a `FeatureRegistryEntry` for every discovered module, capturing:

| Property | Description |
|---|---|
| `Key` | The feature key (e.g. `"Cache"`) |
| `Tier` | `BuiltIn` or `Pluggable` |
| `Order` | Registration and middleware order |
| `Enabled` | Resolved enabled state |
| `Source` | Which provider resolved the flag (`Configuration`, `Environment`, `Override`, `Default`) |
| `PackageId` | The assembly/package that provided the module |
| `Version` | Assembly version |

---

## 7. Diagnostics

### Startup log

`UsePowerFeatures` logs the resolved feature matrix at `Information` level via the `"PowerCSharp.Features"` logger category. Each feature produces one structured log entry:

```
PowerFeature Cache [Pluggable] enabled=True source=Configuration order=100 package=PowerCSharp.Feature.Cache
```

### Diagnostics HTTP endpoint

Opt-in via `options.EnableDiagnosticsEndpoint()` or `PowerFeatures:Diagnostics:Enabled = true`. Returns the feature matrix as JSON:

```http
GET /power-features
```

```json
[
  { "Key": "Cache", "Tier": "Pluggable", "Order": 100, "Enabled": true, "Source": "Configuration", "PackageId": "PowerCSharp.Feature.Cache", "Version": "1.3.0" },
  { "Key": "Cors",  "Tier": "BuiltIn",   "Order": 10,  "Enabled": true, "Source": "Configuration", "PackageId": "PowerCSharp.BuiltInFeatures", "Version": "1.0.0" }
]
```

The endpoint path defaults to `/power-features` and is configurable: `options.EnableDiagnosticsEndpoint("/my-features")`.

---

## 8. Related Documents

- [`PowerCSharp.Features.Architecture.md`](PowerCSharp.Features.Architecture.md) — System design, two tiers, dependency topology
- [`PowerCSharp.Features.Authoring-Guide.md`](PowerCSharp.Features.Authoring-Guide.md) — Step-by-step feature authoring with worked example
- [`PowerCSharp.Features.FlagReference.md`](PowerCSharp.Features.FlagReference.md) — Full flag schema, variants, provider integration
- [`PowerCSharp.Features.PackageLayout.md`](PowerCSharp.Features.PackageLayout.md) — Solution layout and build configuration
- [`PowerCSharp.BuiltInFeatures.md`](PowerCSharp.BuiltInFeatures.md) — Built-in features bundle reference
- [`PowerCSharp.Feature.Cache.md`](PowerCSharp.Feature.Cache.md) — Cache feature family reference
