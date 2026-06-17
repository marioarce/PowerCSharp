# PowerCSharp Features — Package Layout & Build Proposal

> **Status:** Implementation-prep proposal (no code yet). Defines the proposed solution/folder layout for the Features packages, `Directory.Build.props` version additions, the local-NuGet-feed workflow for the `PowerCSharp.CleanArchitecture` template, and README stub templates for each new package.

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
│  └─ Features/                                 (NEW group)
│     ├─ PowerCSharp.Features.Abstractions/     contracts only, zero third-party deps
│     ├─ PowerCSharp.Features/                  engine (discovery, flag resolution, DI, diagnostics)
│     ├─ PowerCSharp.BuiltInFeatures/           Group 1 bundle
│     ├─ PowerCSharp.Feature.Cache/             Group 2 — contracts + module + options + NoOp
│     └─ PowerCSharp.Feature.Cache.BitFaster/   Group 2 — BitFaster implementation (isolated dep)
│
├─ tests/
│  ├─ PowerCSharp.Features.Tests/               engine + abstractions + flag resolution
│  ├─ PowerCSharp.BuiltInFeatures.Tests/
│  └─ PowerCSharp.Feature.Cache.Tests/          incl. isolation/NoOp/provider-selection tests
│
└─ PowerCSharp.sln                              add the new projects + a "Features" solution folder
```

**Solution folder**: add a `Features` solution folder (GUID type `{2150E333-...}`, same as existing `src`/`tests` virtual folders) and nest the new `.csproj` entries under it for IDE grouping.

**Target framework**: `net8.0` (matches the ecosystem). `PowerCSharp.Features.Abstractions` may multi-target `netstandard2.0;net8.0` to maximize reach, mirroring `PowerCSharp.Core`.

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
dotnet add package PowerCSharp.Features            --version 1.0.0-dev.*
dotnet add package PowerCSharp.BuiltInFeatures      --version 1.0.0-dev.*
dotnet add package PowerCSharp.Feature.Cache        --version 1.0.0-dev.*
dotnet add package PowerCSharp.Feature.Cache.BitFaster --version 1.0.0-dev.*
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

### `PowerCSharp.Feature.Cache/README.md`
```markdown
# PowerCSharp.Feature.Cache

Cache feature contracts + module + options + NoOp (no third-party deps). Pair with a
provider package (e.g. PowerCSharp.Feature.Cache.BitFaster) to choose a backend.

- **Package ID:** PowerCSharp.Feature.Cache
- **Depends on:** PowerCSharp.Features.Abstractions
- **Flag:** PowerFeatures:Cache (Enabled, Provider, Capacity, Disk)
- See: docs/PowerCSharp.Features.Authoring-Guide.md
```

### `PowerCSharp.Feature.Cache.BitFaster/README.md`
```markdown
# PowerCSharp.Feature.Cache.BitFaster

BitFaster-backed implementation of PowerCSharp.Feature.Cache. References BitFaster.Caching;
this dependency is isolated here and never enters apps that don't reference this package.

- **Package ID:** PowerCSharp.Feature.Cache.BitFaster
- **Depends on:** PowerCSharp.Feature.Cache + BitFaster.Caching
- **Activate:** PowerFeatures:Cache:Provider = BitFaster
```

---

## 5. Implementation Order (when we start building code)

1. `PowerCSharp.Features.Abstractions` — contracts (`IFeatureModule`, `IFeatureFlagProvider`, `FeatureFlagValue`, `FeatureOptionsBase`, `FeatureDescriptor`).
2. `PowerCSharp.Features` — engine (discovery, composite flag resolver, `FeatureRegistry`, `AddPowerFeatures`/`UsePowerFeatures`, diagnostics + endpoint).
3. `PowerCSharp.BuiltInFeatures` — start with one trivial built-in (e.g. CORS) to exercise the pipeline.
4. `PowerCSharp.Feature.Cache` — contracts + module + options + NoOp.
5. `PowerCSharp.Feature.Cache.BitFaster` — migrate the BitFaster/disk-cache implementation from the source project.
6. Tests for each, including the **isolation validation** described in the Authoring Guide §7.
7. Wire the `PowerCSharp.CleanArchitecture` template to consume via the local feed and validate end-to-end.

> Each step is a separate, reviewable unit of work — no implementation begins until explicitly approved.

---

## 6. Related Documents

- **`PowerCSharp.Features.Architecture.md`**
- **`PowerCSharp.Features.Authoring-Guide.md`**
- **`PowerCSharp.Features.FlagReference.md`**
