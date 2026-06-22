# PowerCSharp.Features.Abstractions

Contracts for the PowerCSharp Features system. Zero third-party dependencies so any feature
can reference it cheaply (it relies only on the shared ASP.NET Core framework).

## Contents

- **`IFeatureModule`** — the unit a feature implements to self-register (`ConfigureServices`, optional `ConfigurePipeline`).
- **`IFeatureFlagProvider`** — flag access with typed variants (`FeatureFlagValue`), not boolean-only.
- **`FeatureFlagValue` / `FeatureFlagSource`** — resolved value plus the source that produced it (for diagnostics).
- **`FeatureOptionsBase`** — base for a feature's typed options bound from `PowerFeatures:<Key>`.
- **`FeatureDescriptor` / `FeatureTier`** — metadata for the registry and diagnostics.
- **`IFeatureRegistrationContext` / `IFeaturePipelineContext`** — wiring surfaces passed to a module.

## Details

- **Package ID:** `PowerCSharp.Features.Abstractions`
- **Dependencies:** none (framework reference only)
- See: `docs/PowerCSharp.Features.Architecture.md`
