# PowerCSharp.Operational — Architecture & Design Rationale

> Why the Operational package family is shaped the way it is, what was deliberately excluded from
> v1, and the decision trail behind each structural choice. Companion to
> [`PowerCSharp.Operational.md`](PowerCSharp.Operational.md) (the API reference).

---

## 1. Purpose

`Operational` is a cross-cutting package family: a shared mechanism for issue/error capture,
in-app diagnostics, structured logging, disk-based event logging, and HTTP resilience — usable
across any application architecture (Clean, Onion, Hexagonal, monolith), safe to enable/disable at
any time without destabilizing the host application, and performance-friendly (background
processing for anything that touches disk).

---

## 2. Where Operational Sits: Beside the Features Framework, Not Inside It

**Decision: Operational is a standalone package family that sits *beside* `PowerCSharp.Features`,
not as a leaf feature inside it.**

### Reasoning

The Features Framework (`PowerCSharp.Features` + `Feature.Cache` + `Feature.Sanitization`) is built
for **optional, leaf capabilities** an application explicitly opts into (a cache backend, a
sanitization engine) via `AddPowerFeatures()` + `PowerFeatures:<Key>:Enabled`. Its
`Feature.Sanitization.Abstractions` package is the closer precedent for Operational's shape: it is
explicitly documented as "usable standalone — no DI or feature registration required."

Operational is different in kind from Cache or Sanitization: it's cross-cutting plumbing (error
capture, structured logging) that *other* code — potentially even Features Framework internals —
may want to call. Forcing every consumer of `IIssueManager`/`ILogger` integration to first stand up
the Features discovery engine would invert the dependency direction cross-cutting concerns are
supposed to have: diagnostics should be available to log a Features Framework startup failure, not
depend on the Features Framework having started successfully first.

So Operational follows the **Sanitization-Abstractions shape**, not the **Cache shape**:

- Fully usable with **zero registration** (NoOp by default) — satisfies "connect/disconnect, app
  keeps working, no crashes."
- *Optionally* wires into `PowerCSharp.Features` (`OperationalFeatureModule`) for config-driven
  enable/disable through the same composite flag chain as Cache and Sanitization, but never
  requires it.

---

## 3. Package Topology & Naming

```
PowerCSharp.Operational.Abstractions   contracts + NoOp defaults, zero third-party deps
                                        (netstandard2.0 + net8.0)
  └─ PowerCSharp.Operational           real implementation, ASP.NET Core-coupled (net8.0 only)
       ├─ PowerCSharp.Operational.Sentry        [future — not in this build]
       └─ PowerCSharp.Operational.WinEventLog   [future — not in this build]
```

`PowerCSharp.Operational.<Provider>` (not `Feature.Operational.*`) is the naming convention for any
future provider package that isolates a third-party or platform-specific dependency — exactly like
`Feature.Cache.BitFaster` isolates `BitFaster.Caching`. Provider packages are named at the same
nesting level as `Operational`, mirroring how `PowerCSharp.Compatibility` sits beside `Core` rather
than under it.

**Versioning:** own family, `PowerCSharpOperationalVersion`, covering `Operational.Abstractions` +
`Operational` together — the same pattern as `PowerCSharpFeatureCacheVersion` covering the whole
Cache family. Future provider packages (Sentry/WinEventLog) get their own version property when
that phase starts, mirroring the Cache/BitFaster precedent.

### Where `IRetryPolicyProvider` lives

`IRetryPolicyProvider` is defined in `PowerCSharp.Operational.Policies.Retry` (the core package),
not `.Abstractions`. Its members return Polly types directly (`ResiliencePipeline<T>`,
`IAsyncPolicy`, `AsyncRetryPolicy`, `RetryPolicy`) — an interface shaped around a third-party
library's types cannot live in a zero-dependency package without leaking that dependency into every
consumer of `.Abstractions`, including ones that never touch retry logic. This is a direct
application of the dependency-isolation rule in §4, applied to the contract itself rather than only
to implementations.

---

## 4. Dependency Isolation

Following the PowerCSharp invariant already enforced for Cache and Sanitization: no third-party or
platform-specific dependency may leak into a consumer that didn't ask for it.

- `Operational.Abstractions` — zero third-party deps (only `Microsoft.Extensions.Logging.Abstractions`).
- `Operational` — ASP.NET Core deps only (`Microsoft.AspNetCore.Http`, `Microsoft.Extensions.*`,
  `Polly`) — no Sentry, no PostSharp, no third-party issue-tracking SDK.
- A future Windows Event Log implementation or third-party issue-tracking integration would live
  **only** in its own provider package (`Operational.WinEventLog`, `Operational.Sentry`) — an app
  that references core `Operational` alone must never pull either in transitively.

---

## 5. Target Frameworks & Platform Scope

- **This build targets ASP.NET Core / Web API only**, matching the source material's actual HTTP
  coupling (`IHttpContextAccessor` used throughout `DiagnosticsService`/`DiagnosticsLogger`/
  `EventLogWriter`). No attempt is made in this phase to decouple from HTTP context.
- `Operational.Abstractions`: `netstandard2.0;net8.0` — usable from .NET Framework consumers too,
  mirroring `Feature.Cache.Abstractions`/`Feature.Sanitization.Abstractions`.
- `Operational`: `net8.0` only (ASP.NET Core ecosystem, via `FrameworkReference Microsoft.AspNetCore.App`).
- A platform-agnostic version (non-web hosts — console apps, workers) is explicitly out of scope
  for v1 and noted as a future phase.

---

## 6. Windows Event Log — Cross-Platform Handling

Windows Event Viewer forwarding is Windows-only by nature. Rather than guard it with
`OperatingSystem.IsWindows()` conditionals inside the core package, `PowerCSharp.Operational` ships
an `IEventViewerService`-shaped **NoOp/pluggable hook only** — no Windows Event Log code lives in
the core package at all. A real Windows implementation would become
`PowerCSharp.Operational.WinEventLog`, a separate provider package, following the same isolation
pattern as `Feature.Cache.BitFaster`. This keeps `Operational.Abstractions` genuinely cross-platform
with no conditional-compilation Windows code inside it — mirroring how `PowerCSharp.Compatibility`
is kept as its own isolated layer rather than `#if` blocks scattered through `Core`.

---

## 7. Enablement Model / NoOp Pattern

- `Operational.Abstractions` ships NoOp implementations of every contract
  (`NoOpDiagnosticsService`, `NoOpIssueManager`, `NoOpEventViewerService`). If nothing is
  registered, calling `IIssueManager.CaptureException(...)` etc. is a safe no-op — this alone
  satisfies "connect/disconnect without crashing."
- `PowerCSharp.Operational` optionally participates in the Features Framework:
  `PowerFeatures:Operational:Enabled` (same shape as `PowerFeatures:Cache:...`/
  `PowerFeatures:Sanitization:...`), resolved through the same composite flag chain (code override
  → `IFeatureFlagProvider` → environment variable → appsettings → default). This is opt-in wiring,
  not a hard dependency — an app can use `Operational` with plain DI registration
  (`AddOperational()`/`UseOperational()`) and never touch the Features engine at all.

---

## 8. Reuse Decisions

- **Sanitization.** `DiagnosticsService`, `DiagnosticsLogger`, and `EventLogWriter`'s
  failure-forwarding path all delegate masking and sensitive-data filtering to the already-shipped
  `PowerCSharp.Feature.Sanitization.Abstractions` package (`Mask`, `SanitizeForLog`,
  `SanitizeForSensitiveData`), rather than reimplementing a sanitization engine. This is the one
  place Operational takes a project reference on another PowerCSharp package rather than standing
  alone.
- **Correlation id.** No PowerCSharp-wide correlation-id abstraction exists yet. Rather than port or
  reimplement one, every call site that would use one (`IssueManager.CaptureException`,
  `DiagnosticsLogger.ForwardToEventViewer`, `EventLogWriter.Enqueue`) carries an explicit `// TODO`
  marking this as a known v1 gap. `EventLogWriter` falls back to `HttpContext.TraceIdentifier` (or a
  fresh GUID when no HTTP context is available) so file names stay unique in the meantime.
- **Static-locator → constructor DI.** The reference implementation resolved several dependencies
  through a static service-locator. Every one of those was converted to constructor DI in this
  port, with one deliberate, narrowly-scoped exception: `DiagnosticsLogger` resolves
  `IDiagnosticsService` from `HttpContext.RequestServices` at each `Log<TState>` call, because
  `ILoggerProvider.CreateLogger` runs once at host startup — outside any request scope — while
  `IDiagnosticsService` is scoped per-request. This is the standard, narrowly-justified pattern for
  bridging a singleton-lifetime `ILogger` to a per-request scoped service, not a general-purpose
  service-locator reintroduction.
- **`EventLogWriter` as a DI singleton, not a static `Instance`.** The reference implementation used
  a hand-rolled static singleton. Every dependency `EventLogWriter` needs
  (`IHttpContextAccessor`, `IEventViewerService`, `OperationalOptions`) is itself singleton-safe, so
  it is registered as a genuine DI singleton (`services.AddSingleton<EventLogWriter>()`) instead —
  a deliberate improvement over the original pattern, not a behavior-preserving port.

---

## 9. Debranding

Applied as a complete strip, not a rename-only pass: no references of any kind — in code, XML doc
comments, default string literals, config keys, file/namespace names, or this documentation — to
the original client, product, or internal ticket system the reference implementation came from. All
`#if DEBUG` / debug-console scaffolding was removed and replaced with proper `ILogger` usage (e.g.
`RetryPolicyProvider`'s unit-test-host detection uses assembly inspection, not a debug-only branch).
Sentry SDK and third-party issue-tracking framework calls were removed entirely — not renamed and
kept — per the explicit exclusion in §10.

---

## 10. Scope

### Ported (debranded, adapted to PowerCSharp conventions)

`IssueManager`, `DiagnosticsService`, `DiagnosticHeaders`, `DiagnosticsLogger` +
`DiagnosticsLoggerProvider` + `NullScope`, `EventLogWriter`, `EventLogRetentionCleaner`,
`RetryPolicyProvider` (+ a newly-authored `IRetryPolicyProvider`, since the original had no
interface split appropriate for this dependency-isolation boundary — see §3).

### Explicitly excluded (not ported at all)

- AOP/aspect-based capture attributes — out of scope per the original requirements brief.
- Third-party issue-tracking SDK calls and references (all of them, everywhere).
- A secondary internal error-reporting framework and all its call sites — removed, not
  debranded-and-kept.
- A test-data-writer utility unrelated to Operational's diagnostics purpose.

### Deferred to later phases (explicitly out of scope for this build)

- `PowerCSharp.Operational.Sentry` provider package.
- `PowerCSharp.Operational.WinEventLog` provider package (§6).
- A platform-agnostic (non-ASP.NET Core) version of `Operational` for console apps/workers/other
  host types.

---

## 11. Decision Log

| # | Decision |
|---|---|
| 1 | Operational is a standalone family beside the Features Framework (§2), not a full Features Framework member. |
| 2 | Full debranding: code, docs, comments — no mentions of any kind (§9). |
| 3 | v1 targets ASP.NET Core/Web API only; a platform-agnostic version is deferred (§5). |
| 4 | Same NoOp/enablement pattern as Cache/Sanitization (§7). |
| 5 | Reuse `PowerCSharp.Feature.Sanitization.Abstractions` for masking/sensitive-data filtering; leave `// TODO` for correlation id (§8). |
| 6 | Target `netstandard2.0;net8.0` for Abstractions; Windows Event Log isolated into its own future provider package, following the `.Compatibility` isolation precedent (§6). |
| 7 | Remove all debug-only scaffolding (`#if DEBUG`, ad-hoc console output). |
| 8 | Own version family (`PowerCSharpOperationalVersion`), same pattern as Cache/Sanitization. |
| 9 | `EventLogRetentionCleaner` and `RetryPolicyProvider` included in v1 scope alongside the core diagnostics/logging/event-log path. |
| 10 | `IRetryPolicyProvider` placed in the core `PowerCSharp.Operational` package, not `.Abstractions`, because its shape depends on Polly types (§3). |
| 11 | Static service-locator resolution converted to constructor DI throughout, except the one narrowly-scoped `DiagnosticsLogger` → `HttpContext.RequestServices` bridge (§8). |
| 12 | `EventLogWriter` registered as a DI singleton rather than a hand-rolled static `Instance` (§8). |
| 13 | New unit tests written from scratch — no pre-existing tests were available to port. |

---

## 12. Open Risks

- **IP clearance.** Bringing debranded logic derived from a production client codebase into a
  public MIT-licensed OSS package requires clearance from that codebase's owner. This document
  records the engineering decisions made assuming that clearance is in place; it is not itself
  evidence that clearance was obtained, and should be confirmed independently before this package is
  published.
- **Correlation-id gap.** Left as a `// TODO` per the decision in §8 — any consumer relying on a
  correlation id linking a request's diagnostic events, log lines, and disk event-log files will not
  get one from `Operational` v1. Revisit once PowerCSharp ships a correlation-id abstraction.
- **No compiler verification at delivery time.** This package's v1 implementation was authored in
  an environment without a .NET SDK available to build or test it. All correctness checking during
  authoring was manual code review. A `dotnet restore && dotnet build && dotnet test` pass against
  `PowerCSharp.sln` is required before merging, to catch anything a compiler would have caught that
  review did not.

---

## 13. Related Documents

- [`PowerCSharp.Operational.md`](PowerCSharp.Operational.md) — API reference
- [`PowerCSharp.Features.Architecture.md`](PowerCSharp.Features.Architecture.md) — Two-tier design, dependency topology
- [`PowerCSharp.Feature.Sanitization.md`](PowerCSharp.Feature.Sanitization.md) — The engine Operational delegates sanitization to
