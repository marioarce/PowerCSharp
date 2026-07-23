# CLAUDE.md — PowerCSharp.Compatibility

> Scope: this project only. Read alongside the root `CLAUDE.md`.

## Sensitivity level: high — actively used in production

Confirmed with the maintainer: this package is **actively depended on by real .NET Framework
consumers**, not a legacy shim kept for completeness. Treat every change as if it ships to
production the moment it's merged. This is the opposite default from "legacy code you can
modernize freely" — the constraint here is backward compatibility, not code quality improvement.

## 1. What Makes This Package Different From the Rest of the Repo

- **Target frameworks:** `net48;net462;net472` only — no `net8.0`, no `netstandard2.0`. This is
  the *only* package in the repo that does not multi-target `netstandard2.0` or `net8.0`.
  (`PowerCSharp.Compatibility.csproj`.)
- **`LangVersion` is pinned to `11.0`**, not `latest` — an explicit override of the repo-wide
  default set in `Directory.Build.props`. Do not "fix" this to match the rest of the repo; it is
  pinned because the target frameworks and consuming apps constrain available language features.
  If a change requires a newer C# feature, that is a signal the change may not belong in this
  package.
- **References classic ASP.NET, not ASP.NET Core:** `System.Web` and `Microsoft.CSharp` are
  referenced per-TFM (`net48`, `net462`, `net472` conditional `ItemGroup`s). `HttpRequestBase` /
  `HttpRequestExtensions` in this package operate against System.Web types
  (`Http/HttpRequestBaseExtensions.cs`), which have no equivalent in, and must never be confused
  with, the ASP.NET Core `HttpRequest` extensions in `PowerCSharp.Extensions.AspNetCore`.
- **Own version family:** `PowerCSharpCompatibilityVersion`, bumped by manual edit to
  `Directory.Build.props` (not currently wired to the `workflow_dispatch` `package_family` choices
  the way `core`/`features`/`cache` are — see Section 3 before assuming otherwise).
- **Package dependencies are deliberately narrow and framework-appropriate**: `System.Text.Json`
  (for `net48`/`net462`/`net472`, where `System.Text.Json` isn't part of the framework), the
  `System.Net.Http` 4.3.4 back-compat package, and `Microsoft.CSharp` for dynamic support. Do not
  add a dependency here that assumes a modern BCL surface is present.

## 2. The CI Gap — Read Before Assuming Test Coverage

- `PowerCSharp.Compatibility.Extensions.Tests` was removed from `PowerCSharp.sln` and from the CI
  build matrix in commit `86464f7` ("ci: revert CI to single Ubuntu runner and remove
  Compatibility.Extensions.Tests from solution").
- The test project now lives **only** in the separate `PowerCSharp-Compatibility.sln`, which
  `.github/workflows/ci-cd.yml` never builds or runs.
- **Practical consequence:** a change to `PowerCSharp.Compatibility` today does not get automatic
  test coverage from the standard `dotnet build/test PowerCSharp.sln` pipeline used everywhere
  else in this repo. If you modify this package:
  - Manually build and run its tests via `PowerCSharp-Compatibility.sln`:
    ```bash
    dotnet test PowerCSharp-Compatibility.sln --configuration Release
    ```
  - Say so explicitly in your response (`<risk>` per the root `CLAUDE.md`) — do not report
    "tests pass" based on the main solution's green CI run, since that run does not exercise this
    package's tests at all.
- This gap is a known, named issue, not something to silently "fix" by re-adding the test project
  to the main solution without asking — the removal in `86464f7` may have been a deliberate,
  considered call (e.g. avoiding a cross-platform matrix cost) rather than an oversight. Flag it
  as an open question rather than reverting it unilaterally.

## 3. Versioning

`PowerCSharpCompatibilityVersion` in `Directory.Build.props` is edited by hand; it is **not** one
of the `core` / `features` / `cache` choices in the `ci-cd.yml` `workflow_dispatch` input today. If
you're asked to release a Compatibility change, confirm with the maintainer whether to extend the
`workflow_dispatch` `package_family` list to include `compatibility`, or continue with the manual
edit + manual tag flow — do not assume either path silently.

## 4. Working in This Package

- Read `docs/PowerCSharp.Compatibility.md` for the documented public surface before adding to it.
- Any new extension method needs the "safe" null-handling style used elsewhere in the repo (see
  `Extensions/StringExtensions.cs` for the existing pattern in this package specifically).
- Do not introduce a dependency on any `net8.0`-only package or language feature — verify
  buildability against all three target frameworks (`net48`, `net462`, `net472`) before considering
  a change complete, using `PowerCSharp-Compatibility.sln` since the main solution's CI won't catch
  a regression here (Section 2).
