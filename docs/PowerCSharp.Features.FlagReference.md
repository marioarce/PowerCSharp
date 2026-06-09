# PowerCSharp Features — Flag Reference

> **Status:** Reference. The complete model for feature flags: configuration schema, typed variants/values, provider precedence, environment-variable conventions, advanced provider integration, and the diagnostics endpoint. See `PowerCSharp.Features.Architecture.md` for the conceptual overview.

---

## 1. Configuration Schema

All feature flags live under the root section **`PowerFeatures`**. Each feature has its own subsection keyed by its `FeatureKey`.

```json
{
  "PowerFeatures": {
    "Diagnostics": { "Enabled": false },

    "Cache": {
      "Enabled": true,
      "Provider": "BitFaster",
      "Capacity": 1000,
      "Disk": { "Path": "./cache", "MaxSizeMb": 512 }
    },

    "Sitecore": {
      "Enabled": false
    },

    "Cors": {
      "Enabled": true,
      "AllowedOrigins": [ "https://example.com" ]
    }
  }
}
```

- **`Enabled`** (bool) — the primary on/off gate. Present on every feature (from `FeatureOptionsBase`).
- **Variant/value properties** — any additional typed properties (string, enum, number, array, nested object) the feature declares in its options class.
- A feature reads its subsection by binding `PowerFeatures:<Key>` to its `FeatureOptionsBase` subclass.

---

## 2. Flags Are Not Boolean-Only

`IFeatureFlagProvider` exposes typed **values/variants** in addition to the boolean gate:

```csharp
bool IsEnabled(string featureKey);
FeatureFlagValue GetValue(string featureKey);
ValueTask<bool> IsEnabledAsync(string featureKey, CancellationToken ct = default);
ValueTask<FeatureFlagValue> GetValueAsync(string featureKey, CancellationToken ct = default);
```

`FeatureFlagValue` provides typed accessors and reports the resolving source:

```csharp
var provider = flags.GetValue("Cache").AsEnum<CacheProvider>();   // BitFaster
var capacity = flags.GetValue("Cache").Property("Capacity").AsInt32();
var source   = flags.GetValue("Cache").Source;                    // e.g. "appsettings"
```

Use cases for non-boolean flags:
- **Provider selection** — `Cache:Provider = BitFaster | Memory`.
- **Multivariate config** — thresholds, capacities, sampling rates.
- **Gradual rollout values** — variant strings interpreted by the feature.

> For most options, bind the whole `PowerFeatures:<Key>` section to your typed options class. Use `GetValue(...)` for cross-feature reads or dynamic decisions outside options binding.

---

## 3. Provider Precedence

Flag values are resolved by a **composite provider** that consults sources in order. The **first source that supplies a value wins**; if none do, the feature default applies.

| # | Source | Precedence | Typical use |
|---|---|---|---|
| 1 | **Explicit code override** | Highest | Tests, hard requirements (`options.Override("Cache", true)`) |
| 2 | **Custom `IFeatureFlagProvider`** | High | Secrets-driven, Azure App Config, LaunchDarkly, DB |
| 3 | **Environment variables** | Medium | Container/CI overrides |
| 4 | **appsettings (`IConfiguration`)** | Low | Default declarative config |
| 5 | **Feature default** (`FeatureDescriptor.DefaultEnabled`) | Lowest | Fallback when unspecified |

- Default install wires sources **3 + 4 + 5** (env vars, appsettings, defaults).
- Sources **1 + 2** are opt-in (host or feature registers them).
- The resolved **source is recorded** for every flag and surfaced in diagnostics.

---

## 4. Environment Variable Convention

Standard .NET double-underscore nesting under the `POWERFEATURES` root:

| Config path | Environment variable |
|---|---|
| `PowerFeatures:Cache:Enabled` | `POWERFEATURES__CACHE__ENABLED` |
| `PowerFeatures:Cache:Provider` | `POWERFEATURES__CACHE__PROVIDER` |
| `PowerFeatures:Cache:Capacity` | `POWERFEATURES__CACHE__CAPACITY` |
| `PowerFeatures:Diagnostics:Enabled` | `POWERFEATURES__DIAGNOSTICS__ENABLED` |

Values are strings; the framework coerces to the target type during binding/`GetValue`.

---

## 5. Advanced Provider Integration

A feature whose availability depends on more than static config (e.g. a secret must exist, or an external flag service) registers its **own `IFeatureFlagProvider`** scoped to its key.

```csharp
public sealed class SitecoreSecretFlagProvider : IFeatureFlagProvider
{
    private readonly ISecretReader _secrets;
    public SitecoreSecretFlagProvider(ISecretReader secrets) => _secrets = secrets;

    public bool IsEnabled(string featureKey)
        => featureKey == "Sitecore" && _secrets.Exists("sitecore/api-key");

    // GetValue / async variants ...
}
```

- Register it in the feature's `ConfigureServices` (or its explicit extension); the engine folds it into the composite resolver at precedence **#2**.
- A single feature may rely on **both** appsettings (`Enabled`, options) **and** its own provider (e.g. appsettings says "on", the provider confirms the secret exists). Both must agree per the precedence rules.
- Keep custom providers **fast and side-effect-free** on the hot path; cache external lookups.

---

## 6. Diagnostics

Two surfaces, both in scope from the first release:

### Startup log
On `AddPowerFeatures`, the engine logs the resolved feature matrix:

```
[PowerFeatures] Resolved 7 features:
  Cache        tier=Pluggable enabled=true  source=appsettings provider=BitFaster
  Sitecore     tier=Pluggable enabled=false source=default     (skipped)
  Cors         tier=BuiltIn   enabled=true  source=appsettings
  ...
```

### HTTP endpoint (opt-in, off by default)
`GET /power-features` returns the matrix as JSON for the running app.

- **Off by default** for safety; enable via `options.EnableDiagnosticsEndpoint()` and/or `PowerFeatures:Diagnostics:Enabled = true`.
- The endpoint is itself a flag-gated feature (`Diagnostics`).
- Recommended to protect it (auth / internal-only) when enabled in non-dev environments.

Example response:

```json
{
  "features": [
    { "key": "Cache",    "tier": "Pluggable", "enabled": true,  "source": "appsettings", "noop": false, "variant": { "Provider": "BitFaster" } },
    { "key": "Sitecore", "tier": "Pluggable", "enabled": false, "source": "default",     "noop": false },
    { "key": "Cors",     "tier": "BuiltIn",   "enabled": true,  "source": "appsettings", "noop": false }
  ]
}
```

---

## 7. Quick Reference

| Task | How |
|---|---|
| Turn a feature on/off | `PowerFeatures:<Key>:Enabled` |
| Override in container/CI | `POWERFEATURES__<KEY>__ENABLED` |
| Force in code/tests | `options.Override("<Key>", true)` |
| Select a backend | variant property, e.g. `PowerFeatures:Cache:Provider` |
| Read a flag in code | `IFeatureFlagProvider.IsEnabled / GetValue` |
| Bind feature options | bind `PowerFeatures:<Key>` to `FeatureOptionsBase` subclass |
| Secret/external-driven flag | register a custom `IFeatureFlagProvider` |
| Inspect resolved state | startup log or `GET /power-features` (opt-in) |

---

## 8. Related Documents

- **`PowerCSharp.Features.Architecture.md`** — conceptual model, package topology, gating, lifecycle.
- **`PowerCSharp.Features.Authoring-Guide.md`** — building a feature, Cache worked example, NoOp pattern.
