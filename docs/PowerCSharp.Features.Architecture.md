# PowerCSharp Features — Architecture

> **Status:** Architecture specification. Defines the Feature & Feature-Flag mechanism for PowerCSharp: how reusable capabilities are packaged, discovered, gated (build-time + runtime), configured, and registered into a host application's DI container and request pipeline.

---

## 1. Goals & Principles

The Features system turns reusable capabilities (cache, sanitization, observability, third-party integrations, etc.) into **self-contained, toggleable units** so that every application can compose exactly the capabilities it needs — nothing more.

- **Self-contained** — a feature owns its contracts, implementation, options, flag, and DI/pipeline wiring.
- **Toggleable** — features enable/disable via flags without code changes in the host.
- **Dependency-isolated** — a feature that pulls a third-party library (e.g. BitFaster, Sitecore SDK) must not impose that dependency on apps that don't use it.
- **Composable** — features register into a single host integration point with deterministic ordering.
- **Observable** — the resolved feature state (enabled/disabled/skipped) is inspectable at startup and at runtime.
- **Convention over configuration** — sensible defaults; advanced scenarios remain possible via explicit hooks.

---

## 2. Two Feature Tiers

| Tier | Name | Packaging | Gating | Third-party deps |
|---|---|---|---|---|
| **Group 1** | **Built-in Features** | One bundle package (`PowerCSharp.BuiltInFeatures`) | Runtime flag only | None isolated — only framework/ASP.NET Core |
| **Group 2** | **Pluggable Features** | One package (or family) each (`PowerCSharp.Feature.<Name>`) | Two-layer: package reference + runtime flag | Isolated per package |

**Built-in Features** are lightweight ASP.NET Core / pipeline / app capabilities (CORS, correlation ID, exception handling, security headers, health checks, sanitization, JWT wiring). They ship together and are toggled purely at runtime.

**Pluggable Features** are complex and/or third-party-bearing capabilities (Cache, Sitecore, Sentry, OpenTelemetry, AWS Secrets). Each is its own package so its dependencies only enter an app that explicitly references it.

> Any Built-in Feature can be disabled and replaced by a custom Pluggable Feature — e.g. ship your own JWT or sanitization feature and turn off the built-in one.

---

## 3. Package Topology

```
PowerCSharp.Features.Abstractions       contracts only, zero third-party deps
PowerCSharp.Features                     engine: discovery, flag resolution, DI orchestration, diagnostics
PowerCSharp.BuiltInFeatures              Group 1 bundle (depends on engine + Microsoft.AspNetCore.App)

PowerCSharp.Feature.Cache.Abstractions   Group 2 — cache contracts + NoOp floors (no third-party, netstandard2.0 + net8.0)
PowerCSharp.Feature.Cache                Group 2 — module + options + ASP.NET Core wiring (net8.0)
PowerCSharp.Feature.Cache.BitFaster      Group 2 — BitFaster-backed implementation (isolates BitFaster.Caching)
PowerCSharp.Feature.Cache.Disk           Group 2 — disk-backed LRU implementation (no third-party)
# future:
PowerCSharp.Feature.Cache.Memory         native MemoryCache implementation
PowerCSharp.Feature.Sitecore            third-party GraphQL integration
PowerCSharp.Feature.Sentry              third-party APM
PowerCSharp.Feature.Observability       OpenTelemetry
```

### Dependency direction

```
                 PowerCSharp.Features.Abstractions   (contracts, no deps)
                        ▲                  ▲
                        │                  │
        PowerCSharp.Features (engine)      │
            ▲           ▲                  │
            │           │                  │
 PowerCSharp.BuiltInFeatures   PowerCSharp.Feature.Cache (module/options)
   (Group 1: bundle)              ▲        ▲
                                 │        │
                                 │        │
        PowerCSharp.Feature.Cache.Abstractions (cache contracts + NoOp)
                        ▲                  ▲
                        │                  │
     PowerCSharp.Feature.Cache.BitFaster   PowerCSharp.Feature.Cache.Disk
        (impl, isolates BitFaster)          (disk LRU impl)

 All features MAY also depend on existing libraries:
   PowerCSharp.Core / .Extensions / .Helpers / .Utilities
```

- **`PowerCSharp.Features.Abstractions`** — pure contracts (`IFeatureModule`, `IFeatureFlagProvider`, options base types, descriptors). No third-party deps so any feature can reference it cheaply.
- **`PowerCSharp.Features`** — the engine: assembly discovery, flag-resolution pipeline, DI orchestration, the `FeatureRegistry`, diagnostics, and the `AddPowerFeatures()` / `UsePowerFeatures()` host entry points.
- **`PowerCSharp.BuiltInFeatures`** — the Group 1 bundle; references the engine + `Microsoft.AspNetCore.App`.
- **Pluggable packages** — reference the framework `Abstractions` and/or the feature-family `Abstractions` package, plus their own isolated third-party deps.
- **`PowerCSharp.Feature.Cache.Abstractions`** — cache-specific contracts (`ICacheService`, `IDiskCacheService`, `CacheProvider`, metadata types) and the NoOp safe-off implementations. Targets `netstandard2.0` and `net8.0` so providers can run on .NET Framework and .NET Core.

> **Namespace note:** because the package name is `PowerCSharp.Feature.Cache.Abstractions`, the contract namespaces are `PowerCSharp.Feature.Cache.Abstractions`, `PowerCSharp.Feature.Cache.Abstractions.Enums`, and `PowerCSharp.Feature.Cache.Abstractions.NoOp`. They are **not** under `PowerCSharp.Feature.Cache`.

### Naming conventions

- Plural **`Features`** = framework (abstractions + engine).
- **`PowerCSharp.BuiltInFeatures`** = the single Group 1 bundle.
- Singular **`Feature.<Name>`** = a Pluggable Feature module/options package.
- **`Feature.<Name>.Abstractions`** = the contracts and safe-off NoOp implementations for a pluggable feature; zero third-party dependencies and the widest target-framework reach.
- **Feature family** for swappable backends: `Feature.<Name>.Abstractions` (contracts/NoOp) + `Feature.<Name>` (module/options) + `Feature.<Name>.<Provider>` (implementation). Providers are **provider-named, not type-named** (e.g. `.BitFaster`, `.Memory`), matching .NET norms such as `Microsoft.Extensions.Caching.StackExchangeRedis`.

---

## 4. Core Contracts (`PowerCSharp.Features.Abstractions`)

Conceptual shapes (names indicative; see the Authoring Guide for usage).

### `IFeatureModule`
The unit a feature implements to self-register.

```csharp
public interface IFeatureModule
{
    string FeatureKey { get; }   // stable identifier, e.g. "Cache"
    int Order { get; }           // registration + middleware ordering (lower runs first)

    void ConfigureServices(IFeatureRegistrationContext context);   // DI registration
    void ConfigurePipeline(IFeaturePipelineContext context);       // optional middleware (mostly Group 1)
}
```

### `IFeatureRegistrationContext`
Passed to `ConfigureServices`; wraps the wiring surface.

- `IServiceCollection Services`
- `IConfiguration Configuration`
- `IFeatureFlagProvider Flags` — resolved flag access for conditional sub-registration
- `ILogger Logger`
- `FeatureDescriptor Descriptor`

### `IFeaturePipelineContext`
Passed to `ConfigurePipeline`; wraps `IApplicationBuilder` and resolved services.

### `IFeatureFlagProvider`
Flag access — **not boolean-only**. Supports typed variants/values from day one.

```csharp
public interface IFeatureFlagProvider
{
    bool IsEnabled(string featureKey);
    FeatureFlagValue GetValue(string featureKey);            // typed variant: string/enum/number
    ValueTask<bool> IsEnabledAsync(string featureKey, CancellationToken ct = default);
    ValueTask<FeatureFlagValue> GetValueAsync(string featureKey, CancellationToken ct = default);
}
```

`FeatureFlagValue` exposes typed accessors (`AsBoolean()`, `AsString()`, `AsEnum<T>()`, `AsInt32()`, …) plus the **source** that produced it (for diagnostics).

### `FeatureOptionsBase`
Base for a feature's typed options bound from its config section; always carries `Enabled`.

```csharp
public abstract class FeatureOptionsBase
{
    public bool Enabled { get; set; }
}
```

### `FeatureDescriptor`
Metadata for the registry/diagnostics: `Key`, `DisplayName`, `Tier` (BuiltIn/Pluggable), `DefaultEnabled`, `PackageId`, `Version`.

---

## 5. Discovery & Registration (Hybrid)

Two registration styles are supported; **a feature author picks the one that suits its nature** (or supports both).

1. **Auto-discovery** — the feature ships an `IFeatureModule`; the engine scans candidate assemblies and registers all discovered + enabled modules. Minimal host wiring.
2. **Explicit** — the feature ships a `services.Add<Name>Feature(configuration)` extension; the host calls it directly (no reflection). Preferred where the author wants full control or to avoid scanning costs.

- **Built-in Features** lean on auto-discovery within the bundle assembly.
- **Pluggable Features** commonly expose an explicit extension *and* an `IFeatureModule`, so hosts can choose convenience or control.

Discovery is **opt-in per assembly** — the host declares which assemblies to scan in `AddPowerFeatures(...)`, so nothing is registered by surprise.

---

## 6. Two-Layer Gating

Pluggable Features (Group 2) are gated by two independent layers; both must pass for a feature to run.

### Layer 1 — Build-time (dependency isolation)
If the host does **not** reference `PowerCSharp.Feature.<Name>` (and its `.Provider` impl), the module type and all of its third-party dependencies are simply **absent** from the dependency tree — zero code, zero transitive packages. This is the **primary** isolation mechanism.

### Layer 2 — Runtime (flag)
If a feature package **is** referenced but its flag is **off**, the module is discovered but its `ConfigureServices` is **skipped**. Where dependents need a binding regardless, the feature registers a **NoOp implementation** so resolution stays safe (mirrors the proven `NoOpDiskCacheService` fallback pattern from the source project).

```
Reference package?   Flag enabled?   Result
       No                 —          Absent: no code, no deps
       Yes                No         Discovered, skipped; optional NoOp registered
       Yes                Yes        Active: ConfigureServices + ConfigurePipeline run
```

**Built-in Features use Layer 2 only** — they always ship in the bundle and are runtime-toggled.

---

## 7. Feature-Flag Resolution

### Configuration shape (appsettings — default source)

```json
{
  "PowerFeatures": {
    "Cache":    { "Enabled": true,  "Provider": "BitFaster", "Capacity": 1000 },
    "Sitecore": { "Enabled": false }
  }
}
```

### Provider chain (composite, ordered — highest precedence wins)

1. **Explicit code override** — host sets a flag in the `AddPowerFeatures` callback.
2. **Custom/advanced `IFeatureFlagProvider`** — opt-in per feature (e.g. secret-driven, Azure App Config).
3. **Environment variables** — convention `POWERFEATURES__<KEY>__ENABLED` (and `__<PROPERTY>` for variants).
4. **appsettings** — `PowerFeatures:<Key>:Enabled` (+ variant properties).
5. **Feature default** — `FeatureDescriptor.DefaultEnabled`.

- The engine composes all registered providers into a **composite resolver**. A default install wires only the `ConfigurationFeatureFlagProvider` (appsettings) + the environment-variable provider.
- A single feature can require **both** appsettings (simple flags/options) **and** its own provider (e.g. a feature that reads a secret to decide availability). It registers an additional provider scoped to its feature key.
- **Variants/values** (not just on/off) are first-class — used for provider selection and multivariate config (e.g. `Cache:Provider = BitFaster | Memory`).

See `PowerCSharp.Features.FlagReference.md` for the full schema, precedence rules, and provider integration.

---

## 8. Host Integration & Lifecycle

```csharp
// Program.cs
builder.Services
    .AddPowerFeatures(builder.Configuration, options =>
    {
        options.ScanAssemblies(typeof(CacheFeatureModule).Assembly); // opt-in auto-discovery
        options.Override("Cache", true);                              // optional explicit override
        options.EnableDiagnosticsEndpoint();                          // opt-in, off by default
    })
    .AddCacheFeature(builder.Configuration);                          // optional explicit Group-2 registration

var app = builder.Build();

app.UsePowerFeatures();   // applies enabled features' middleware in Order
```

**`AddPowerFeatures`**:
1. Builds the composite flag resolver from registered providers.
2. Discovers `IFeatureModule`s in the opted-in assemblies (+ any explicitly added).
3. Filters modules by resolved enabled-state.
4. Invokes each enabled module's `ConfigureServices`.
5. Records a `FeatureRegistry` capturing every feature's state: **discovered / enabled / skipped / NoOp**, plus the **source** of its flag value.

**`UsePowerFeatures`**:
- Invokes `ConfigurePipeline` for each enabled, middleware-bearing feature in ascending `Order`.

---

## 9. Diagnostics

In scope from the first release:

- **Startup log** — a structured summary of the resolved feature matrix (key, tier, enabled, source, NoOp?).
- **Opt-in HTTP endpoint** — e.g. `GET /power-features` returning the same matrix as JSON for the running app. The endpoint is **itself flag-gated and off by default** for safety; enable explicitly (`options.EnableDiagnosticsEndpoint()` and/or `PowerFeatures:Diagnostics:Enabled`).

This visibility is especially valuable for the public Clean Architecture template, where consumers need to see exactly which features are active.

---

## 10. Versioning

- **`PowerCSharpFeaturesVersion`** — single shared version for the **framework trio** (`Features.Abstractions` + `Features` + `BuiltInFeatures`), which evolve together.
- **Per-pluggable-feature version** — each Pluggable Feature has its own version variable (e.g. `PowerCSharpFeatureCacheVersion`), since they ship and evolve independently. Within a family, the contracts package and its provider implementations may share that feature's version.
- All version variables live in `Directory.Build.props` alongside the existing `PowerCSharpVersion`.

---

## 11. Relationship to the Roadmap

- **(a) Miscellaneous code** — extensions/helpers/utilities already exist (`PowerCSharp.Core/.Extensions/.Helpers/.Utilities`); features build on top of them.
- **(b) Features** — this system; the unit of reusable, toggleable capability.
- **(c) Clean Architecture WebApi template** (`PowerCSharp.CleanArchitecture`) — consumes the framework + selected feature packages. `AddPowerFeatures()` + chosen `PowerCSharp.Feature.*` becomes the starting point for new applications. Built in parallel as a live integration harness (private first, public when stable), consuming PowerCSharp packages via a **local NuGet feed** to validate real packaging + dependency isolation.

---

## 12. Related Documents

- **`PowerCSharp.Features.Authoring-Guide.md`** — how to build a new feature (Group 1 vs Group 2 decision tree, module contract, options, flags, NoOp pattern) with the Cache family as the worked example.
- **`PowerCSharp.Features.FlagReference.md`** — flag schema, variants, provider precedence, env-var conventions, advanced provider integration, diagnostics endpoint.
