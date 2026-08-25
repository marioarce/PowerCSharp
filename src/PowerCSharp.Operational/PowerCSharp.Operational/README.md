# PowerCSharp.Operational

![PowerCSharp Banner](https://raw.githubusercontent.com/marioarce/PowerCSharp/0191ee12092c28ccf5a578e59977583117a3ff00/docs/images/PowerCSharp_Banner.png)

Cross-cutting issue capture, in-app diagnostics, structured logging, disk event-log writing, and
HTTP retry/circuit-breaker resilience for ASP.NET Core applications — safe to enable or disable at
any time.

Pair this package with `PowerCSharp.Operational.Abstractions` (contracts + NoOp) for the
cross-platform contract surface.

## Contents

- **`DiagnosticsService`** — per-request diagnostics: trace/breadcrumb/exception/error capture,
  activated via HTTP headers (`debug`, `debugVerbose`, `traceLevel`, `eventLog`, `cacheDisabled`,
  `performance` — see `DiagnosticHeaders`), with sensitive-data masking delegated to
  `PowerCSharp.Feature.Sanitization.Abstractions`.
- **`IssueManager`** — centralized exception/error/breadcrumb capture, forwarding to `IDiagnosticsService`.
- **`DiagnosticsLogger`** / **`DiagnosticsLoggerProvider`** — a custom `ILogger` that forwards to
  diagnostics and (optionally) a platform event log.
- **`EventLogWriter`** / **`EventLogRetentionCleaner`** — background NDJSON disk writer with
  cross-process file locking, and a companion retention-cleanup sweep.
- **`RetryPolicyProvider`** — Polly-based retry + circuit breaker for outbound HTTP calls, using
  decorrelated jitter backoff.
- **`OperationalServiceCollectionExtensions`** — `AddOperational()` / `UseOperational()` explicit DI wiring.
- **`OperationalFeatureModule`** — optional `PowerCSharp.Features` auto-discovery module.

## Usage

```csharp
// Program.cs
builder.Services.AddOperational(builder.Configuration);

var app = builder.Build();
app.UseOperational();
```

```json
// appsettings.json
{
  "PowerFeatures": {
    "Operational": {
      "Enabled": true,
      "LogsBasePath": "C:\\Logs",
      "LogsRetentionDays": 30
    }
  }
}
```

Then, from anywhere with constructor access to `IIssueManager` / `IDiagnosticsService`:

```csharp
public class SomeService(IIssueManager issueManager, IDiagnosticsService diagnostics)
{
    public async Task DoWorkAsync()
    {
        try
        {
            diagnostics.AddBreadcrumb("Starting work", category: "SomeService");
            // ...
        }
        catch (Exception ex)
        {
            issueManager.CaptureException(ex);
            throw;
        }
    }
}
```

### Optional: Features Framework integration

```csharp
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.ScanAssemblies(typeof(OperationalFeatureModule).Assembly);
});
```

## Design notes

- **No static service locator.** Unlike the source implementation this package was built from,
  `IDiagnosticsService` and `IIssueManager` are constructor-injected. The one deliberate exception
  is `DiagnosticsLogger`, which resolves `IDiagnosticsService` from `HttpContext.RequestServices` at
  each log call — the standard, narrowly-scoped pattern for bridging a singleton-lifetime `ILogger`
  to per-request scoped services, not a general-purpose locator.
- **`EventLogWriter` is a DI singleton**, not a hand-rolled static `Instance` — every dependency it
  needs is itself singleton-safe.
- **Correlation id is a known v1 gap**, marked with `// TODO` at each call site. No PowerCSharp
  correlation-id abstraction exists yet; this is tracked for a future phase rather than silently
  dropped or half-ported from a Sentry-coupled origin.
- **No Windows Event Log code lives in this package.** `IEventViewerService` defaults to
  `NoOpEventViewerService`; a future `PowerCSharp.Operational.WinEventLog` provider package supplies
  the real Windows implementation. See `docs/PowerCSharp.Operational.Architecture.md`.

## Details

- **Package ID:** `PowerCSharp.Operational`
- **Depends on:** `PowerCSharp.Operational.Abstractions`, `PowerCSharp.Features.Abstractions`, `PowerCSharp.Feature.Sanitization.Abstractions`, `Polly`
- **Target framework:** `net8.0` (requires an ASP.NET Core host)
