# Workflows.md

> Standard Operating Procedures for recurring tasks in the PowerCSharp repository. These SOPs are
> the concrete, step-by-step companion to `CLAUDE.md`. Follow them in order; do not skip
> verification steps to save time.

---

## SOP-01 — Adding a New Extension / Utility / Helper Method

**When to use:** adding a method to an existing `PowerCSharp.Extensions`, `PowerCSharp.Utilities`,
`PowerCSharp.Helpers`, or `PowerCSharp.Extensions.AspNetCore` class (or a new class within one of
those packages). This is the `PowerCSharpVersion` family.

1. **Locate the right home.** Match the method to an existing namespace/class by convention
   (e.g. string helpers → `PowerCSharp.Extensions` `StringExtensions`; validation →
   `PowerCSharp.Utilities` `ValidationUtility`). Read the target file fully before adding to it —
   check `docs/PowerCSharp.<Package>.md` for the documented surface and
   `docs/EDGE_CASES_AND_SECURITY.md` for existing null/edge-case conventions in that area.
2. **Match existing signatures and null-handling style.** This codebase favors "safe" variants
   that don't throw (`SafeSubstring`, `FirstOrDefaultSafe`) — follow the established pattern for
   the class you're extending rather than introducing a new error-handling style.
3. **Target frameworks.** Confirm the package's `<TargetFrameworks>` in its `.csproj`
   (`net8.0;netstandard2.0` for most Core-family packages). If the API you need doesn't exist on
   `netstandard2.0`, either polyfill it or guard with `#if NET8_0_OR_GREATER` — do not silently
   drop a target framework.
4. **Nullable + EditorConfig.** `Nullable` is `enable` repo-wide (`Directory.Build.props`). Follow
   `.editorconfig` member-ordering and style rules; run `dotnet format` if unsure.
5. **Write the method with full XML doc comments** (`<summary>`, `<param>`, `<returns>`,
   `<exception>` where relevant) — these packages ship with `GenerateDocumentationFile` and their
   docs pages are hand-maintained from these comments.
6. **Add tests** in the matching `tests/PowerCSharp.<Package>.Tests` project (xunit). Cover: happy
   path, null/empty input, and at least one boundary case, mirroring the style already used for
   sibling methods in the same test file.
7. **Update package docs.** Add a short usage example to `docs/PowerCSharp.<Package>.md` and, if
   the method is a headline addition, to the "Usage Examples" section of the root `README.md`.
8. **Update `CHANGELOG.md`** under an `[Unreleased]` or next-version heading, following the
   Keep a Changelog format already in use.
9. **Verify locally:**
   ```bash
   dotnet build PowerCSharp.sln --configuration Release
   dotnet test tests/PowerCSharp.<Package>.Tests --configuration Release
   ```
10. **Version family:** this change belongs to `PowerCSharpVersion` (or
    `PowerCSharpCompatibilityVersion` if the target is `PowerCSharp.Compatibility` — see that
    package's own `CLAUDE.md` first). Do not bump the version yourself; that happens in SOP-03 at
    release time via `workflow_dispatch`. Note the intended family in your summary to the reviewer.
11. **Branch and hand off.** Work on `feature/<short-description>` branched from `develop`. Do not
    merge or push without explicit approval — present the diff for peer review per the working
    agreement in place for this repository.

---

## SOP-02 — Building a New Pluggable Feature Package (`PowerCSharp.Feature.<Name>`)

**When to use:** standing up a new feature under the Features Framework — for example, the
roadmapped `PowerCSharp.Feature.Sitecore`. Read `docs/PowerCSharp.Features.Architecture.md` and
`docs/PowerCSharp.Features.Authoring-Guide.md` in full before starting; this SOP sequences that
guidance into an actionable checklist. Do not begin writing code before both documents have been
read for this session.

1. **Classify the tier.** Run the decision tree in the Authoring Guide §1. A feature that pulls a
   third-party SDK (a Sitecore GraphQL/Content Serialization client, for example) or carries
   non-trivial implementation is **always Pluggable (Group 2)** — it never belongs in
   `PowerCSharp.BuiltInFeatures`.
2. **Decide single package vs. family.** If there is exactly one implementation, use a single
   `PowerCSharp.Feature.<Name>` package. If there are or will be swappable backends (as with
   Cache: BitFaster vs. Disk), split into `PowerCSharp.Feature.<Name>` (contracts + module) plus
   one `PowerCSharp.Feature.<Name>.<Provider>` package per backend. **Ask the maintainer if this is
   ambiguous** — do not guess for a feature with real architectural weight like Sitecore.
3. **Scaffold the project(s)** under `src/Features/PowerCSharp.Feature.<Name>[.{Provider}]/`,
   matching `docs/PowerCSharp.Features.PackageLayout.md` §1 folder conventions:
   - Contracts-only packages target `netstandard2.0;net8.0` and take **zero** third-party
     dependencies beyond `Microsoft.Extensions.Logging.Abstractions`.
   - Implementation packages target `net8.0` and isolate their third-party SDK dependency
     entirely within that package (Section 3.2 of the root `CLAUDE.md`).
4. **Define the anatomy** (Authoring Guide §2), all four/five pieces:
   - a stable `FeatureKey` string (e.g. `"Sitecore"`), used in config as `PowerFeatures:<Key>`;
   - the contracts other code will depend on;
   - a `FeatureOptionsBase` subclass bound from `PowerFeatures:<Key>`;
   - an `IFeatureModule` implementation for auto-discovery, plus (optionally) an explicit
     `Add<Name>Feature()` extension method for opt-in registration;
   - a safe-off (NoOp) behavior for when the flag is disabled — dependents must still resolve.
5. **Wire `ConfigureServices`** using the Cache module (`CacheFeatureModule`, in
   `src/Features/PowerCSharp.Feature.Cache/`) as the literal template: check
   `context.Flags.IsEnabled(FeatureKey)` first, register the NoOp implementation and return early
   if disabled, otherwise register the real implementation. Add `ConfigurePipeline` only if the
   feature contributes middleware.
6. **Add the project(s) to `PowerCSharp.sln`** under the existing `Features` solution folder, plus
   a matching `tests/PowerCSharp.Feature.<Name>.Tests` project referencing `xunit` +
   `Microsoft.NET.Test.Sdk` at the versions already used by sibling test projects.
7. **Add a new version-family property** to `Directory.Build.props`
   (`PowerCSharpFeature<Name>Version`, e.g. `PowerCSharpFeatureSitecoreVersion`), starting at
   `1.0.0`. Set every new package's `<PackageVersion>` to that property. Add the family to the
   `package_family` choice list in `.github/workflows/ci-cd.yml`'s `workflow_dispatch` inputs.
8. **Write tests** covering: flag-off → NoOp is resolved; flag-on → real implementation is
   resolved; option binding; and the feature's own core logic. Mirror
   `tests/PowerCSharp.Feature.Cache.Tests` structure.
9. **Document it**: a package `README.md` (packed via `PackageReadmeFile`, see the Cache `.csproj`
   for the pattern), an entry in the root `README.md` package table, and a `docs/PowerCSharp.Feature.<Name>.md`
   page following the existing per-package doc format.
10. **Verify dependency isolation manually.** Reference only the new package from a scratch console
    app and confirm the third-party SDK does *not* appear in `dotnet list package --include-transitive`
    output unless that specific package was referenced.
11. **Branch and hand off.** Work on `feature/<name>-feature` from `develop`. Present the plan
    (tier decision, package layout, version-family name) as a `<plan>` block per `CLAUDE.md`
    Section 5 before writing substantial code, since this is an architectural addition — confirm
    with the maintainer before scaffolding if any step above required a judgment call.

---

## SOP-03 — Preparing and Publishing a Release

**When to use:** a `develop`-branch feature set is ready to ship to NuGet.org and GitHub Packages
for one or more version families. Full reference: `docs/WORKFLOW.md`.

1. **Confirm `develop` is green.** `dotnet build PowerCSharp.sln --configuration Release` and
   `dotnet test PowerCSharp.sln --configuration Release` must both pass locally before starting;
   CI will re-verify on push but don't rely on CI to catch avoidable failures.
2. **Identify which version family(ies) changed** since the last release: `core`
   (`PowerCSharpVersion`), `features` (`PowerCSharpFeaturesVersion`), `cache`
   (`PowerCSharpFeatureCacheVersion`), or a newly-introduced family from SOP-02. Cross-check
   against `CHANGELOG.md`'s `[Unreleased]` section.
3. **Create the release branch from `develop`:**
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b release/v<X.Y.Z>
   ```
4. **Update documentation and changelog** on the release branch: move `[Unreleased]` entries in
   `CHANGELOG.md` under the new version heading with today's date, and confirm the root
   `README.md` NuGet badge table doesn't need a new row (new package) or wording updates.
5. **Bump the version(s).** Preferred path is the `workflow_dispatch` trigger on
   `.github/workflows/ci-cd.yml` (`release_type`: patch/minor/major, `package_family`:
   core/features/cache/\<new family\>) — this both edits `Directory.Build.props` and creates the
   `v<X.Y.Z>` / `features-v<X.Y.Z>` / `cache-v<X.Y.Z>` tag automatically. If multiple families
   changed, run it once per family. Manual edits to `Directory.Build.props` are the fallback only
   if `workflow_dispatch` is unavailable.
6. **Open the PR:** `release/v<X.Y.Z>` → `main`, including release notes, a summary of testing
   performed, and any breaking changes called out explicitly (per `CLAUDE.md` Section 2.9).
7. **On merge to `main`**, the pipeline automatically: builds and tests, packs (`dotnet pack
   PowerCSharp.sln --configuration Release --output ./packages`), publishes to NuGet.org and
   GitHub Packages, and deletes the `release/*` branch. Confirm the packages appear on
   nuget.org/GitHub Packages after the workflow completes — do not assume a green pipeline run
   equals a successful publish without checking.
8. **Verify Codecov** picked up the coverage report for the release build (badge in `README.md`).
9. **Do not commit, tag, or push any step above without explicit sign-off** unless the user has
   directed you to execute the release directly — treat release actions as high-blast-radius by
   default and confirm before executing steps 3, 5, 6, and 7.
