# PowerCSharp Features — Package Layout & Build Proposal

> **Status:** Implemented. Defines the solution/folder layout for the Features packages, `Directory.Build.props` version additions, the local-NuGet-feed workflow for the `PowerCSharp.CleanArchitecture` template, and README reference for each package. All packages described here are shipped as of v2.0.0.

---

## 1. Proposed Solution & Folder Layout

New projects slot into the existing `src/` + `tests/` convention. The Features packages are grouped under `src/Features/` for clarity.

```
PowerCSharp/
├─ src/
│  ├─ PowerCSharp.Core/                         (existing)
│  ├─ PowerCSharp.Extensions/                   (existing)
│  ├─ PowerCSharp.Extensions.AspNetCore/        (existing)
│  ├─ PowerCSharp.Helpers/                      (existing)
│  ├─ PowerCSharp.Utilities/                    (existing)
│  ├─ PowerCSharp.Compatibility/                (existing)
│  │
│  └─ Features/                                     (NEW group)
│     ├─ PowerCSharp.Features.Abstractions/         contracts only, zero third-party deps
│     ├─ PowerCSharp.Features/                      engine (discovery, flag resolution, DI, diagnostics)
│     ├─ PowerCSharp.BuiltInFeatures/               Group 1 bundle
│     ├─ PowerCSharp.Feature.Cache.Abstractions/    Group 2 — contracts + NoOp (zero third-party deps)
│     ├─ PowerCSharp.Feature.Cache/                 Group 2 — module + options + ASP.NET Core wiring
│     ├─ PowerCSharp.Feature.Cache.BitFaster/       Group 2 — BitFaster implementation (isolated dep)
│     └─ PowerCSharp.Feature.Cache.Disk/            Group 2 — disk-backed LRU implementation
│
├─ tests/
│  ├─ PowerCSharp.Features.Tests/               engine + abstractions + flag resolution
│  ├─ PowerCSharp.BuiltInFeatures.Tests/
│  ├─ PowerCSharp.Feature.Cache.Tests/          incl. isolation/NoOp/provider-selection tests
│  └─ PowerCSharp.Feature.Cache.Abstractions.Tests/  (optional) contracts + NoOp behavior tests
│
└─ PowerCSharp.sln                              add the new projects + a "Features" solution folder
```

**Solution folder**: add a `Features` solution folder (GUID type `{2150E333-...}`, same as existing `src`/`tests` virtual folders) and nest the new `.csproj` entries under it for IDE grouping.

**Target frameworks**:
- `net8.0` for the engine, built-ins, and feature modules (matches the ASP.NET Core ecosystem).
- `netstandard2.0;net8.0` for feature abstractions packages (e.g. `PowerCSharp.Features.Abstractions`, `PowerCSharp.Feature.Cache.Abstractions`) so providers and console apps can run on .NET Framework and .NET Core.

---

## 2. `Directory.Build.props` Additions

Add new version variables alongside the existing ones (current: `PowerCSharpVersion = 2.0.0`). Framework trio shares one version; each pluggable feature versions independently.

```xml
<Project>
  <PropertyGroup>
    <!-- existing -->
    <PowerCSharpVersion>2.0.0</PowerCSharpVersion>
    <PowerCSharpCompatibilityVersion>1.0.0</PowerCSharpCompatibilityVersion>

    <!-- NEW: framework trio (Abstractions + Features + BuiltInFeatures) -->
    <PowerCSharpFeaturesVersion>1.0.0</PowerCSharpFeaturesVersion>

    <!-- NEW: per-pluggable-feature versions -->
    <PowerCSharpFeatureCacheVersion>1.0.0</PowerCSharpFeatureCacheVersion>
    <!-- future: PowerCSharpFeatureSitecoreVersion, PowerCSharpFeatureSentryVersion, ... -->

    <!-- existing common metadata (Authors, Copyright, License, etc.) unchanged -->
  </PropertyGroup>
</Project>
```

Each feature `.csproj` sets `<PackageVersion>$(PowerCSharpFeaturesVersion)</PackageVersion>` or `$(PowerCSharpFeatureCacheVersion)` accordingly. The Cache family (`Feature.Cache` + `Feature.Cache.BitFaster`) shares `PowerCSharpFeatureCacheVersion`.

---

## 3. Local NuGet Feed Workflow (for the template repo)

The `PowerCSharp.CleanArchitecture` template consumes PowerCSharp packages **via a local folder feed** (not `ProjectReference`) so packaging + dependency isolation are validated authentically.

### One-time setup
```bash
# create a local folder feed
mkdir -p ~/.powercsharp-local-nuget
```

### Pack from PowerCSharp (repeat after changes)
```bash
# from the PowerCSharp repo root; use a -dev suffix to dodge NuGet caches
dotnet pack -c Release \
  -p:PackageVersionSuffix=dev.$(date +%Y%m%d%H%M%S) \
  -o ~/.powercsharp-local-nuget
```
> Note: a `<VersionSuffix>`/`PackageVersionSuffix` hook may need to be added to the feature `.csproj`/props; alternatively bump the `-dev` semantic version manually.

### `NuGet.Config` in the template repo (`PowerCSharp.CleanArchitecture`)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="powercsharp-local" value="~/.powercsharp-local-nuget" />
  </packageSources>
</configuration>
```

### Reference from the template
```bash
dotnet add package PowerCSharp.Features                     --version 1.0.0-dev.*
dotnet add package PowerCSharp.BuiltInFeatures              --version 1.0.0-dev.*
dotnet add package PowerCSharp.Feature.Cache.Abstractions   --version 1.0.0-dev.*
dotnet add package PowerCSharp.Feature.Cache                --version 1.0.0-dev.*
dotnet add package PowerCSharp.Feature.Cache.BitFaster      --version 1.0.0-dev.*
dotnet add package PowerCSharp.Feature.Cache.Disk           --version 1.0.0-dev.*
```

**Inside the PowerCSharp solution itself**, use `ProjectReference` between feature projects for fast inner-loop development; only the template consumes via the feed.

---

## 4. README Stub Templates

Each new package gets a `README.md` packed into its NuGet (`<PackageReadmeFile>README.md</PackageReadmeFile>`), following the existing badge/section style.

### `PowerCSharp.Features.Abstractions/README.md`
```markdown
# PowerCSharp.Features.Abstractions

Contracts for the PowerCSharp Features system: IFeatureModule, IFeatureFlagProvider,
FeatureOptionsBase, FeatureDescriptor. Zero third-party dependencies so any feature can
reference it cheaply.

- **Package ID:** PowerCSharp.Features.Abstractions
- **Dependencies:** None
- See: docs/PowerCSharp.Features.Architecture.md
```

### `PowerCSharp.Features/README.md`
```markdown
# PowerCSharp.Features

The Features engine: feature discovery (hybrid auto-scan + explicit), flag resolution
(composite provider chain), DI orchestration, FeatureRegistry, and diagnostics.
Entry points: AddPowerFeatures() / UsePowerFeatures().

- **Package ID:** PowerCSharp.Features
- **Depends on:** PowerCSharp.Features.Abstractions
- See: docs/PowerCSharp.Features.Architecture.md, FlagReference.md
```

### `PowerCSharp.BuiltInFeatures/README.md`
```markdown
# PowerCSharp.BuiltInFeatures

Bundle of lightweight, runtime-flag-toggled ASP.NET Core capabilities: CORS, correlation
ID, exception handling, security headers, health checks, sanitization, JWT wiring.
Toggle each via PowerFeatures:<Key>:Enabled. Any built-in can be disabled and replaced
by a custom Pluggable Feature.

- **Package ID:** PowerCSharp.BuiltInFeatures
- **Depends on:** PowerCSharp.Features + Microsoft.AspNetCore.App
```

### `PowerCSharp.Feature.Cache.Abstractions/README.md`
```markdown
# PowerCSharp.Feature.Cache.Abstractions

Framework-agnostic cache contracts and NoOp floors for the PowerCSharp Cache feature.
No third-party dependencies; targets netstandard2.0 and net8.0.

- **Package ID:** PowerCSharp.Feature.Cache.Abstractions
- **Depends on:** Microsoft.Extensions.Logging.Abstractions
- **Namespaces:** PowerCSharp.Feature.Cache.Abstractions (and .Enums, .NoOp)
- See: docs/PowerCSharp.Features.Authoring-Guide.md
```

### `PowerCSharp.Feature.Cache/README.md`
```markdown
# PowerCSharp.Feature.Cache

Cache feature module + options + ASP.NET Core wiring. Pair with the abstractions package
and a provider package (e.g. PowerCSharp.Feature.Cache.BitFaster) to choose a backend.

- **Package ID:** PowerCSharp.Feature.Cache
- **Depends on:** PowerCSharp.Features.Abstractions + PowerCSharp.Feature.Cache.Abstractions
- **Flag:** PowerFeatures:Cache (Enabled, Provider, Capacity, Disk)
- See: docs/PowerCSharp.Features.Authoring-Guide.md
```

### `PowerCSharp.Feature.Cache.BitFaster/README.md`
```markdown
# PowerCSharp.Feature.Cache.BitFaster

BitFaster-backed implementation of the PowerCSharp Cache feature. References BitFaster.Caching;
this dependency is isolated here and never enters apps that don't reference this package.

- **Package ID:** PowerCSharp.Feature.Cache.BitFaster
- **Depends on:** PowerCSharp.Feature.Cache.Abstractions + BitFaster.Caching
- **Activate:** PowerFeatures:Cache:Provider = BitFaster
```

### `PowerCSharp.Feature.Cache.Disk/README.md`
```markdown
# PowerCSharp.Feature.Cache.Disk

Disk-backed LRU implementation of the PowerCSharp Cache feature. Targets netstandard2.0
and net8.0; no third-party dependencies beyond framework packages.

- **Package ID:** PowerCSharp.Feature.Cache.Disk
- **Depends on:** PowerCSharp.Feature.Cache.Abstractions
- **Activate:** call services.AddCacheDisk(configuration) or set PowerFeatures:Cache:Provider = Disk
```

---

## 5. Implementation Order (when we start building code)

1. `PowerCSharp.Features.Abstractions` — framework contracts (`IFeatureModule`, `IFeatureFlagProvider`, `FeatureFlagValue`, `FeatureOptionsBase`, `FeatureDescriptor`).
2. `PowerCSharp.Features` — engine (discovery, composite flag resolver, `FeatureRegistry`, `AddPowerFeatures`/`UsePowerFeatures`, diagnostics + endpoint).
3. `PowerCSharp.BuiltInFeatures` — start with one trivial built-in (e.g. CORS) to exercise the pipeline.
4. `PowerCSharp.Feature.Cache.Abstractions` — cache contracts + metadata + NoOp floors.
5. `PowerCSharp.Feature.Cache` — module + options + `AddCacheFeature` extension.
6. `PowerCSharp.Feature.Cache.BitFaster` — BitFaster-backed in-memory provider.
7. `PowerCSharp.Feature.Cache.Disk` — disk-backed LRU provider.
8. Tests for each, including the **isolation validation** described in the Authoring Guide §7.
9. Wire the `PowerCSharp.CleanArchitecture` template to consume via the local feed and validate end-to-end.

> Each step is a separate, reviewable unit of work — no implementation begins until explicitly approved.

---

## 6. Related Documents

- **`PowerCSharp.Features.Architecture.md`**
- **`PowerCSharp.Features.Authoring-Guide.md`**
- **`PowerCSharp.Features.FlagReference.md`**
