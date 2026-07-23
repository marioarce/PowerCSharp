# CLAUDE.md — Features Framework (`src/Features/`)

> Scope: `PowerCSharp.Features.Abstractions`, `PowerCSharp.Features`, `PowerCSharp.BuiltInFeatures`,
> and the `PowerCSharp.Feature.*` family. Read this alongside the root `CLAUDE.md` and, before any
> substantial change here, `docs/PowerCSharp.Features.Architecture.md` and
> `docs/PowerCSharp.Features.Authoring-Guide.md` in full.

## Sensitivity level: high

The engine (`PowerCSharp.Features`) is load-bearing for every feature package that exists or will
exist, including the roadmapped `PowerCSharp.Feature.Sitecore`. A subtle change to discovery or
flag-resolution order silently changes behavior for every consumer, in every host app, without a
compile error. Treat changes here as you would changes to a dependency-injection container:
narrowly scoped, heavily tested, and called out explicitly as `<risk>` in your response.

## 1. The Two-Tier Model

| Tier | Package | Gating | Third-party deps |
|---|---|---|---|
| Group 1 — Built-in | `PowerCSharp.BuiltInFeatures` | Runtime flag only | None isolated (framework/ASP.NET Core only) |
| Group 2 — Pluggable | `PowerCSharp.Feature.<Name>[.Provider]` | Package reference **and** runtime flag | Isolated per package |

Any Built-in Feature can be disabled and replaced by a custom Pluggable Feature. Do not add a
third-party dependency to `PowerCSharp.BuiltInFeatures` — that would violate the tier's contract;
a feature needing one belongs in Group 2.

## 2. Dependency Direction (do not invert)

```text
PowerCSharp.Features.Abstractions        (contracts, zero deps)
        ▲                    ▲
        │                    │
PowerCSharp.Features    PowerCSharp.Feature.<Name>  (module/options)
   (engine)                   ▲
        ▲                     │
        │          PowerCSharp.Feature.<Name>.<Provider>  (isolates 3rd-party SDK)
PowerCSharp.BuiltInFeatures
```

`Features.Abstractions` must never gain a dependency on `Features` (the engine) or on any
`Feature.*` package — it is the floor everything else builds on. If you find yourself wanting to
reference the engine from Abstractions, the abstraction belongs somewhere else.

## 3. Engine Internals — What Each Piece Does

Grounded in the current implementation (`src/Features/PowerCSharp.Features/`):

- **`PowerFeaturesServiceCollectionExtensions.AddPowerFeatures`** — the single entry point. It (in
  order): builds the flag-provider chain, discovers modules, and for **every** discovered module —
  enabled or not — invokes `ConfigureServices`. Modules self-gate internally (Model A: "always
  invoke `ConfigureServices`; the module decides active vs. NoOp"). Do not change this to
  conditionally skip disabled modules' `ConfigureServices` — that would break the NoOp
  safe-off contract every existing feature module relies on.
- **`FeatureModuleDiscovery.Discover`** (`Internal/FeatureModuleDiscovery.cs`) — reflection-based:
  scans opted-in assemblies for public, non-abstract types implementing `IFeatureModule` with a
  parameterless constructor, merges with explicitly-supplied instances (explicit wins on type
  collision), de-dupes by concrete `Type`, and orders by `Order` then `FeatureKey`. Uses
  `Assembly.GetTypes()` wrapped in a `ReflectionTypeLoadException` catch — if you touch this,
  preserve that catch; it exists because partially-loadable assemblies are a real occurrence in
  consumer apps.
- **`CompositeFeatureFlagProvider`** (`Flags/CompositeFeatureFlagProvider.cs`) — precedence-ordered
  chain, first provider to return `HasValue == true` wins. Order is assembled in
  `AddPowerFeatures` as: **Override → custom providers (`options.AdditionalProviders`) →
  Environment → Configuration**. This order is a deliberate product decision (explicit overrides,
  e.g. for tests, must always win over configuration). Do not reorder without discussing the
  consequence for every existing consumer's `appsettings.json` expectations.
- **`IFeatureModule`** (`Features.Abstractions/IFeatureModule.cs`) — the contract every feature
  implements: `FeatureKey` (stable string, maps to `PowerFeatures:<Key>`), `Order` (registration
  ordering, lower first), `ConfigureServices` (required), `ConfigurePipeline` (default no-op).
  A new feature package's module is the concrete implementation of this interface — see
  `docs/PowerCSharp.Features.Authoring-Guide.md` §3.2 for the canonical worked example
  (`CacheFeatureModule`).

## 4. Adding a New Pluggable Feature Here

Do not improvise. Follow SOP-02 in the root `Workflows.md`, which sequences the Authoring Guide
into a checklist. The short version: classify the tier, decide single-package vs. family, scaffold
under `src/Features/PowerCSharp.Feature.<Name>[.Provider]/`, implement the five-piece anatomy
(`FeatureKey`, contracts, options, module, NoOp), and give the family its own
`PowerCSharpFeature<Name>Version` in `Directory.Build.props`.

## 5. Sitecore — Roadmap Status

`docs/PowerCSharp.Features.Architecture.md` §3 lists `PowerCSharp.Feature.Sitecore` (third-party
GraphQL integration) as a future pluggable package. As of this writing:

- It is **confirmed in scope** as a future initiative — not abandoned, not yet started.
- There is a branch, `feature/sitecore/phase-1-implementation`, whose diff against `develop`
  contains **no Sitecore implementation code** — only test-file deletions that look like stale
  rebase artifacts. Do not treat that branch as a starting point without first confirming with the
  maintainer whether it should be discarded or salvaged; assume discard-and-restart via SOP-02
  unless told otherwise.
- No design decisions have been made yet about which Sitecore integration surface this targets
  (Content Serialization, GraphQL Layout Service, Experience Edge, or a combination). Do not
  invent this design — it is an open architectural question to raise with the maintainer before
  scaffolding `PowerCSharp.Feature.Sitecore`, per Workflows.md SOP-02 step 2 and step 11.
