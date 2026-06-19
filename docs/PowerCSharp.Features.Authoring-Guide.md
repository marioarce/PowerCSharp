# PowerCSharp Features — Authoring Guide

> **Status:** Authoring guide. A practical, step-by-step recipe for building a new PowerCSharp Feature, using the **Cache family** as the canonical worked example. Read `PowerCSharp.Features.Architecture.md` first for the conceptual model.

---

## 1. Decide the Tier

Use this decision tree before writing anything.

```
Does the feature pull a third-party library OR carry significant/complex implementation?
        │
        ├── No  → Built-in Feature (Group 1)
        │         Lives in PowerCSharp.BuiltInFeatures. Runtime-flag toggled only.
        │
        └── Yes → Pluggable Feature (Group 2)
                  Its own package (or family). Two-layer gated (package + flag).
                  Third-party deps isolated in the package.
                          │
                          └── Are there swappable backends (e.g. BitFaster vs native)?
                                  │
                                  ├── No  → single package: PowerCSharp.Feature.<Name>
                                  └── Yes → family:
                                            PowerCSharp.Feature.<Name>            (contracts + module)
                                            PowerCSharp.Feature.<Name>.<Provider> (each implementation)
```

| Question | Built-in (G1) | Pluggable (G2) |
|---|---|---|
| Third-party dependency? | No | Yes (isolated) |
| Implementation complexity | Low | Medium/High |
| Packaging | Shared bundle | Own package/family |
| Gating | Flag only | Package + flag |
| Examples | CORS, correlation ID, security headers, sanitization, JWT wiring | Cache, Sitecore, Sentry, OpenTelemetry, AWS Secrets |

---

## 2. Anatomy of a Feature

Every feature, regardless of tier, provides:

1. **A stable `FeatureKey`** — e.g. `"Cache"`. Used in config (`PowerFeatures:Cache`), flags, env vars, and diagnostics.
2. **Contracts** — the interfaces the host/other features depend on (e.g. `ICacheService`).
3. **Options** — a `FeatureOptionsBase` subclass bound from the feature's config section.
4. **A registration mechanism** — an `IFeatureModule` (auto-discovery) and/or an explicit `Add<Name>Feature()` extension.
5. **A safe-off behavior** — what happens when the flag is off (skip, or register a NoOp).
6. **(Optional) Pipeline wiring** — middleware via `ConfigurePipeline` (mostly Group 1).

---

## 3. Worked Example — The Cache Family

The Cache feature is packaged as a **family** to demonstrate the full framework: contracts are split into a zero-dependency abstractions package, the module/options live in the ASP.NET Core package, and each backend is isolated in its own provider package.

```
PowerCSharp.Feature.Cache.Abstractions  → contracts + NoOp floors (NO third-party deps, netstandard2.0 + net8.0)
PowerCSharp.Feature.Cache               → module + options + AddCacheFeature (ASP.NET Core, net8.0)
PowerCSharp.Feature.Cache.BitFaster     → BitFaster-backed implementation (isolates BitFaster.Caching)
PowerCSharp.Feature.Cache.Disk          → disk-backed LRU implementation (no third-party deps)
```

### 3.1 Contracts package — `PowerCSharp.Feature.Cache.Abstractions`

**Dependencies:** `Microsoft.Extensions.Logging.Abstractions` only.

**Namespaces:**

- `PowerCSharp.Feature.Cache.Abstractions` — `ICacheService`, `IDiskCacheService`, `CacheResult<T>`, metadata types.
- `PowerCSharp.Feature.Cache.Abstractions.Enums` — `CacheProvider`, `CacheResultReason`, `CacheEntryPriority`.
- `PowerCSharp.Feature.Cache.Abstractions.NoOp` — `NoOpCacheService`, `NoOpDiskCacheService`.

**Contracts** (modeled on the source project's `Infrastructure/Services/Cache/*`):

```csharp
using PowerCSharp.Feature.Cache.Abstractions;

public interface ICacheService
{
    bool TryGet<T>(string key, out T value);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    void Remove(string key);
}

public interface IDiskCacheService
{
    ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default);
    ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default);
}
```

**Options** — bound from `PowerFeatures:Cache`:

```csharp
using PowerCSharp.Feature.Cache;
using PowerCSharp.Feature.Cache.Abstractions.Enums;

public sealed class CacheFeatureOptions : FeatureOptionsBase
{
    public CacheProvider Provider { get; set; } = CacheProvider.None; // variant flag drives selection
    public int Capacity { get; set; } = 1000;
    public DiskCacheOptions Disk { get; set; } = new();
}

// Defined in PowerCSharp.Feature.Cache.Abstractions.Enums
public enum CacheProvider { None, BitFaster, Disk, Memory }
```

### 3.2 Module package — `PowerCSharp.Feature.Cache`

**Dependencies:** `PowerCSharp.Features.Abstractions` + `PowerCSharp.Feature.Cache.Abstractions`.

**Module** — supports BOTH auto-discovery and explicit registration:

```csharp
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Abstractions.NoOp;
using PowerCSharp.Features.Abstractions;

public sealed class CacheFeatureModule : IFeatureModule
{
    public string FeatureKey => "Cache";
    public int Order => 100;

    public void ConfigureServices(IFeatureRegistrationContext context)
    {
        // Bind + validate options
        var options = context.Configuration
            .GetSection($"PowerFeatures:{FeatureKey}")
            .Get<CacheFeatureOptions>() ?? new CacheFeatureOptions();

        // Layer 2 (flag off): register NoOp so dependents always resolve safely.
        if (!context.Flags.IsEnabled(FeatureKey))
        {
            context.Services.AddSingleton<ICacheService, NoOpCacheService>();
            context.Services.AddSingleton<IDiskCacheService, NoOpDiskCacheService>();
            return;
        }

        // Provider selection via the variant flag/option.
        // NOTE: concrete provider registration lives in the provider package
        //       (e.g. AddCacheBitFaster). This module only wires options and the NoOp floor.
    }

    public void ConfigurePipeline(IFeaturePipelineContext context) { /* no middleware */ }
}
```

**Explicit extension** (optional convenience):

```csharp
using PowerCSharp.Feature.Cache;

public static class CacheFeatureExtensions
{
    public static IServiceCollection AddCacheFeature(this IServiceCollection services, IConfiguration configuration)
        => services.Configure<CacheFeatureOptions>(configuration.GetSection("PowerFeatures:Cache"));
}
```

### 3.3 Implementation package — `PowerCSharp.Feature.Cache.BitFaster`

**Dependencies:** `PowerCSharp.Feature.Cache.Abstractions` + `BitFaster.Caching` (**the isolated third-party**).

```csharp
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.BitFaster;

public static class CacheBitFasterExtensions
{
    // Called by the host when it chooses the BitFaster provider, or by the module
    // when Provider == BitFaster. BitFaster types are ONLY referenced here.
    public static IServiceCollection AddCacheBitFaster(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("PowerFeatures:Cache").Get<CacheFeatureOptions>()!;

        services.AddLru<string, object>(b => b.WithCapacity(options.Capacity).Build());
        services.AddSingleton<ICacheService, BitFasterCacheService>();
        return services;
    }
}
```

> The BitFaster reference exists **only** in this package. An app that does not reference `PowerCSharp.Feature.Cache.BitFaster` never pulls `BitFaster.Caching` — Layer 1 isolation in action.

### 3.4 What the Cache family demonstrates

| Mechanism | How |
|---|---|
| **Layer 1 isolation** | Don't reference `.BitFaster` → `BitFaster.Caching` absent from the dependency tree. |
| **Layer 2 flag** | Reference it but flag off → `NoOp*` registered (mirrors source `NoOpDiskCacheService`). |
| **Non-boolean flag** | `Provider` variant (`BitFaster`/`Disk`/`Memory`) selects the implementation. |
| **Swappable backend** | A future `PowerCSharp.Feature.Cache.Memory` drops in without touching contracts. |
| **Hybrid registration** | `CacheFeatureModule` (auto) + `AddCacheFeature`/`AddCacheBitFaster`/`AddCacheDisk` (explicit). |

---

## 4. The NoOp Pattern (Safe-Off)

When a feature has dependents that always resolve a contract, register a **NoOp** implementation in the flag-off path so the container never fails. NoOps should:

- Implement the full contract with inert behavior (cache misses, no-ops, empty results).
- Log once at `Information`/`Warning` that the feature is disabled.
- Live in the **contracts package** (no third-party deps).

If a contract is only resolved by the feature's own active code, you may skip the NoOp and simply not register anything when the flag is off.

---

## 5. Step-by-Step Checklist

### Built-in Feature (Group 1)
- [ ] Add an `IFeatureModule` to `PowerCSharp.BuiltInFeatures` with a unique `FeatureKey`.
- [ ] Add a `FeatureOptionsBase` subclass bound from `PowerFeatures:<Key>`.
- [ ] Implement `ConfigureServices` (and `ConfigurePipeline` if middleware).
- [ ] Honor the flag: skip or register inert behavior when disabled.
- [ ] Set a sensible `Order` (middleware ordering matters).
- [ ] Add XML docs on public types; add a section to the bundle README.

### Pluggable Feature (Group 2)
- [ ] Create `PowerCSharp.Feature.<Name>.Abstractions` (contracts + NoOp), depending only on framework `Abstractions` (and minimal third-party deps such as logging).
- [ ] Create `PowerCSharp.Feature.<Name>` (module + options + registration extensions), depending on `PowerCSharp.Features.Abstractions` + `PowerCSharp.Feature.<Name>.Abstractions`.
- [ ] If swappable backends: create `PowerCSharp.Feature.<Name>.<Provider>` for each implementation; isolate third-party deps there. Each provider depends only on `PowerCSharp.Feature.<Name>.Abstractions`, not on the module package.
- [ ] Provide both an `IFeatureModule` (in the module package) and explicit `Add<Name>Feature()` / `Add<Name><Provider>()` extensions.
- [ ] Implement Layer 2 flag-off behavior (NoOp where needed).
- [ ] Add a per-feature version variable to `Directory.Build.props` (e.g. `PowerCSharpFeature<Name>Version`). Share it across `Abstractions`, module, and all providers in the family.
- [ ] Add a `README.md` to each package; add the feature to the catalog.
- [ ] Add tests; validate isolation by building a consumer that does NOT reference the provider package.

---

## 6. Conventions

- **FeatureKey** — PascalCase, stable, matches the config section (`PowerFeatures:<Key>`).
- **Options** — always extend `FeatureOptionsBase`; never read raw config strings outside binding.
- **No magic strings** — keep keys/section names as constants.
- **Async** — `CancellationToken` last; `ConfigureAwait(false)` in library code.
- **Logging** — typed `ILogger<T>`; never `Console.Write`.
- **Third-party references** — only ever inside a `Feature.<Name>.<Provider>` package, never in contracts or the bundle.
- **XML docs** — on all public contracts and registration extensions.

---

## 7. Validating Isolation (must-do for Group 2)

Because dependency isolation is the headline benefit, prove it:

1. In the `PowerCSharp.CleanArchitecture` template (or a scratch consumer), reference **only** `PowerCSharp.Feature.Cache` + `PowerCSharp.Feature.Cache.Abstractions` (not `.BitFaster`).
2. Build and inspect the dependency tree (`dotnet list package --include-transitive`).
3. Confirm `BitFaster.Caching` is **absent**.
4. Add `PowerCSharp.Feature.Cache.BitFaster`, rebuild, confirm it now appears — and that toggling the flag off swaps in the NoOp at runtime.
5. (Optional) Prove providers can be used without the ASP.NET Core module: reference only `PowerCSharp.Feature.Cache.Abstractions` + `PowerCSharp.Feature.Cache.BitFaster` in a console app and confirm `PowerCSharp.Feature.Cache` (and `Microsoft.AspNetCore.App`) is **not** required.

---

## 8. Related Documents

- **`PowerCSharp.Features.Architecture.md`** — conceptual model, package topology, gating, lifecycle.
- **`PowerCSharp.Features.FlagReference.md`** — flag schema, variants, provider precedence, diagnostics.
