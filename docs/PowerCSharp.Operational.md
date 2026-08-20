# PowerCSharp.Operational — API Reference

> The Operational package family: cross-cutting issue capture, in-app diagnostics, structured
> logging, disk event-log writing, and HTTP retry/circuit-breaker resilience. Each package in the
> family versions independently under `PowerCSharpOperationalVersion`. Migrated and de-branded from
> an internal reference implementation, then adapted to PowerCSharp's dependency-isolation and
> NoOp-safe-off conventions.

---

## Package Family Overview

| Package | Role | Target Frameworks | Version |
|---|---|---|---|
| `PowerCSharp.Operational.Abstractions` | Contracts, models, enums, NoOp floor | `netstandard2.0` + `net8.0` | `$(PowerCSharpOperationalVersion)` |
| `PowerCSharp.Operational` | Diagnostics, issue capture, logging, event-log writer, retry/circuit-breaker | `net8.0` (ASP.NET Core) | `$(PowerCSharpOperationalVersion)` |

### Dependency direction

```
PowerCSharp.Operational.Abstractions   (contracts + models + NoOp, zero third-party deps)
          ▲
          │
PowerCSharp.Operational                 (real implementations; Polly, ASP.NET Core)
```

`IRetryPolicyProvider` lives in `PowerCSharp.Operational` (not `.Abstractions`) — its members are
shaped in terms of Polly types (`ResiliencePipeline<T>`, `IAsyncPolicy`, `AsyncRetryPolicy`,
`RetryPolicy`), and putting it in `.Abstractions` would leak that third-party dependency into
consumers who only want the zero-dependency contracts. This is the one place the Operational family
departs from "every public contract lives in `.Abstractions`" — see
[`PowerCSharp.Operational.Architecture.md`](PowerCSharp.Operational.Architecture.md) for the full
rationale.

---

## 1. PowerCSharp.Operational.Abstractions

### `IDiagnosticsService`

Per-request (scoped) diagnostics: records trace/breadcrumb/exception/error events and returns a
filtered, sanitized snapshot. Every add-event method is safe to call unconditionally — when
diagnostics are disabled for the current context, they no-op and return `null`.

```csharp
public interface IDiagnosticsService
{
    bool IsEnabled { get; }
    bool IsVerbose { get; }
    int TraceLevel { get; }
    bool IsEventLogEnabled { get; }
    bool IsCacheDisabled { get; }
    bool IsDebugVerbose { get; }
    bool IsPerformanceEnabled { get; }

    DiagnosticEvent? AddTrace(string message, TraceLevel level = TraceLevel.Error, object? data = null, bool obfuscateMessage = false);
    DiagnosticEvent? AddBreadcrumb(string message, string category, BreadcrumbLevel level = BreadcrumbLevel.Info, bool obfuscateMessage = false);
    DiagnosticEvent? AddException(Exception ex, object? data = null);
    DiagnosticEvent? AddError(string message, object? data = null, bool obfuscateMessage = false);
    List<DiagnosticEvent>? GetEvents();
    DiagnosticsPayload? BuildPayload();
    object? Obfuscate(object? input);
}
```

Enabled state and trace level are resolved once per request, from headers (see
[Diagnostic headers](#diagnostic-headers) below) — not from configuration alone. This lets an
engineer or QA tester turn on diagnostics for a single request without touching configuration or
redeploying.

### `IIssueManager`

The shared capture surface: enriches exceptions/errors/breadcrumbs with caller context
(`[CallerMemberName]`/`[CallerFilePath]`/`[CallerLineNumber]`) and forwards them to
`IDiagnosticsService`.

```csharp
public interface IIssueManager
{
    (T Exception, DiagnosticEvent? DiagnosticEvent) CaptureException<T>(
        T ex, object? data = null,
        [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        where T : Exception;

    DiagnosticEvent? CaptureError(
        string message, object? data = null,
        [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);

    DiagnosticEvent? AddBreadcrumb(string message, string category = "general", BreadcrumbLevel level = BreadcrumbLevel.Info, IDictionary<string, string>? data = null);
}
```

Implementations must never throw as a result of capturing an issue — a failure to capture must
never surface as a failure of the operation being diagnosed. `IssueManager` (the real
implementation) enforces this by wrapping every forward-to-diagnostics call in a try/catch that
logs a warning and continues.

### `IEventViewerService`

A pluggable hook for forwarding sanitized log entries to a platform event log (e.g. the Windows
Event Viewer). The core package ships only `NoOpEventViewerService` — no platform-specific code
lives in the cross-platform core. A future `PowerCSharp.Operational.WinEventLog` provider package
supplies the real Windows implementation and overrides this floor.

```csharp
public interface IEventViewerService
{
    bool TryEnqueue(EventViewerLogEntry entry);
}
```

### Models

```csharp
public class DiagnosticEvent
{
    public long Timestamp { get; }              // Unix epoch milliseconds
    public DiagnosticEventType Type { get; set; }
    public string Message { get; set; }
    public string? StackTrace { get; set; }
    public object? Data { get; set; }
    public TraceLevel? TraceLevel { get; set; }
}

public class DiagnosticsPayload
{
    public List<DiagnosticEvent>? Events { get; set; }
}

public record EventViewerLogEntry(
    DateTime Timestamp, string Category, LogLevel Level, string Message,
    string? CorrelationId, string? ExceptionText, IDictionary<string, string>? Tags = null);

[AttributeUsage(AttributeTargets.Property)]
public sealed class SensitiveDataAttribute(int length = 10, char maskChar = '*') : Attribute
{
    public int Length { get; }
    public char MaskChar { get; }
}
```

`SensitiveDataAttribute` is applied by hosts to their own POCOs; `DiagnosticsService.Obfuscate`
reads it via reflection when masking captured `data` objects, using the attribute's `Length`/
`MaskChar` instead of the default mask shape.

### Enums

```csharp
public enum TraceLevel { None = 0, Trace = 1, Debug = 2, Information = 3, Warning = 4, Error = 5, Fatal = 6 }
public enum BreadcrumbLevel { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
public enum DiagnosticEventType { Trace, Breadcrumb, Exception, Error }
```

`BreadcrumbLevel` is defined locally rather than reused from a third-party enum — the original
reference implementation used a Sentry-owned type here, which would have pulled a hidden
third-party dependency into `.Abstractions`.

### `OperationalOptions`

Bound from `PowerFeatures:Operational` (or supplied directly to `AddOperational`).

```csharp
public sealed class OperationalOptions
{
    public bool Enabled { get; set; } = true;
    public string? LogsBasePath { get; set; }
    public int LogsRetentionDays { get; set; } = 30;
    public string? AppName { get; set; }
    public LogLevel DefaultLogLevel { get; set; } = LogLevel.Warning;
    public int DefaultHttpMaxAttempts { get; set; } = 2;
    public int DefaultMethodMaxAttempts { get; set; } = 2;
}
```

`LogsBasePath` gates disk event logging: unset (the default), `EventLogWriter.Enqueue` is inert.

### NoOp implementations

| Class | Interface | Behavior |
|---|---|---|
| `NoOpDiagnosticsService` | `IDiagnosticsService` | Always reports disabled; every add-event method returns `null`; `Obfuscate` returns input unchanged. |
| `NoOpIssueManager` | `IIssueManager` | Exceptions pass through unmodified with no diagnostic event produced. |
| `NoOpEventViewerService` | `IEventViewerService` | `TryEnqueue` always returns `true` — nothing downstream to overflow. |

### Diagnostic headers

Defined in `PowerCSharp.Operational.DiagnosticHeaders` (core package, not Abstractions — these are
HTTP-specific and only meaningful once ASP.NET Core is in play):

| Header | Effect |
|---|---|
| `debug: true` | Enables diagnostics for the request. |
| `debugVerbose: true` | Enables verbose diagnostics (disables auto-obfuscation); requires `debug: true` as well for `IsDebugVerbose`. |
| `traceLevel: <0-6>` | Sets the minimum trace level for the request (see `TraceLevel` enum). |
| `eventLog: true` | Enables disk event-log writing for the request (still requires `LogsBasePath` to be configured). |
| `cacheDisabled: true` | Signals cache bypass for the request (read by cache-aware call sites; Operational itself does not enforce this). |
| `performance: true` | Enables performance-profiling flags for the request. |

---

## 2. PowerCSharp.Operational

### `DiagnosticsService : IDiagnosticsService`

Scoped (per-request) implementation. Reads the headers above from `IHttpContextAccessor` once
(`Initialize()`), records events in a `ConcurrentBag<DiagnosticEvent>`, and filters/sanitizes on
read (`GetEvents()`/`BuildPayload()`).

Key behaviors:

- **Errors force full trace on read.** If any `Error`/`Exception` event was captured, `GetEvents()`
  escalates to full trace output regardless of the configured `traceLevel` header, so
  troubleshooting has the complete picture leading up to the failure.
- **Auto-obfuscation.** Messages matching an IP address, URL, email address, or GUID pattern are
  masked automatically even when the caller didn't request it, unless `IsDebugVerbose`.
- **Sanitization is delegated**, not reimplemented: masking uses
  `PowerCSharp.Feature.Sanitization.Abstractions`' `Mask(char)`/`Mask(int, char)` extensions, and
  sensitive-data filtering on read uses `SanitizeForSensitiveData()`. Operational does not ship its
  own masking engine.
- **Static defaults.** `DefaultLogLevel`, `DefaultHttpMaxAttempts`, `DefaultMethodMaxAttempts` are
  static properties, set from the most recently constructed instance's `OperationalOptions`. This
  lets `DiagnosticsLogger` and `RetryPolicyProvider` — which are not always resolvable within the
  same request scope — read the configured defaults without a direct dependency.

```csharp
public DiagnosticsService(IHttpContextAccessor httpContextAccessor, IOptions<OperationalOptions> options, EventLogWriter eventLogWriter);
```

### `IssueManager : IIssueManager`

Constructor-injects `IDiagnosticsService` and `ILogger<IssueManager>`. `CaptureException<T>` merges
a supplied `data` dictionary into `ex.Data` before forwarding, and enriches the diagnostic event
with `caller = "{file}:{line} ({member})"`. A `// TODO` marks where a request/operation correlation
id will attach once PowerCSharp ships a correlation-id abstraction — flagged as a known v1 gap, not
silently dropped.

### `EventLogWriter`

Writes diagnostic events to disk as NDJSON, one background task processing a `BlockingCollection`
queue so callers never block on disk I/O. Registered as a DI singleton (not a hand-rolled static
`Instance`) — every dependency here (`IHttpContextAccessor`, `IEventViewerService`,
`OperationalOptions`) is itself singleton-safe.

- Files are organized by `{LogsBasePath}/{AppName}/{yyyy}/{MM}/{dd}/log_..._{correlationId}.ndjson`.
- Per-file writes are serialized with a `ConcurrentDictionary<string, object>` of file locks, so
  concurrent requests writing to the same file don't corrupt it.
- `Enqueue` is inert (no-op) until `LogsBasePath` is configured.
- On a write failure, the entry is forwarded through `IEventViewerService` as a best-effort
  fallback channel, in addition to being logged via `ILogger`.
- `Dispose()` calls `CompleteAdding()` on the queue, letting the background task drain and exit.

### `EventLogRetentionCleaner` (static)

Deletes date-partitioned log directories older than `LogsRetentionDays`, on a background `Task.Run`,
tolerating locked files and permission errors (retried on the next pass rather than throwing).
Invoked once at startup by `UseOperational()`.

### `DiagnosticsLogger : ILogger` / `DiagnosticsLoggerProvider : ILoggerProvider`

A custom logging sink that forwards `ILogger` calls to `IDiagnosticsService` (in-app diagnostics)
and `IEventViewerService` (platform event log). Because `ILoggerProvider.CreateLogger` runs once at
host startup — outside any request scope — `DiagnosticsLogger` deliberately resolves
`IDiagnosticsService` from `HttpContext.RequestServices` at each `Log<TState>` call, rather than
through constructor injection. This is the standard pattern for bridging a singleton-lifetime
`ILogger` to a per-request scoped service, and is the one place in this package that reads from
`RequestServices` directly rather than via constructor DI.

Every message is sanitized (`SanitizeForLog().SanitizeForSensitiveData()`) before it reaches
diagnostics, disk, or the platform event log.

### `IRetryPolicyProvider` / `RetryPolicyProvider`

```csharp
public interface IRetryPolicyProvider
{
    ResiliencePipeline<HttpResponseMessage> GetPipeline();
    IAsyncPolicy CreatePolicy(string key);
    AsyncRetryPolicy GetAsyncPolicy(ILogger? logger, int maxAttempts);
    RetryPolicy GetPolicy(ILogger? logger, int maxAttempts);
}
```

`GetPipeline()` returns a Polly v8 `ResiliencePipeline<HttpResponseMessage>` combining exponential
backoff with jitter (decorrelated-jitter shape) and a circuit breaker. Only transient failures are
retried: 5xx responses, `408 Request Timeout`, `429 Too Many Requests`, and
`HttpRequestException` — permanent client failures (400/401/403/404/409/422, etc.) are never
retried. `CreatePolicy`/`GetAsyncPolicy`/`GetPolicy` use the legacy `Polly.Policy` API for
general-purpose (non-HTTP) method retries.

Under a detected unit-test host (xUnit/NUnit/MSTest assembly present in the current
`AppDomain`), `GetPipeline()` builds a no-op pipeline instead — tests exercising code that calls
through the pipeline don't pay for real backoff delays. The legacy `Policy`-based methods are not
test-host-aware; avoid triggering their retry path (i.e., don't assert on failure/retry behavior)
in fast unit tests.

### `OperationalServiceCollectionExtensions`

```csharp
IServiceCollection AddOperational(this IServiceCollection services, IConfiguration configuration);
IApplicationBuilder UseOperational(this IApplicationBuilder app);
```

Explicit (no-reflection) registration — works standalone, no dependency on `PowerCSharp.Features`.
Binds `OperationalOptions` from `PowerFeatures:Operational`. When `Enabled` is `false`, registers
NoOp floors for every contract instead: no background writer task starts, no custom
`ILoggerProvider` is added, and the host behaves exactly as if Operational were never referenced.
`UseOperational` eagerly resolves `EventLogWriter` (starting its background task at startup rather
than lazily) and runs one round of `EventLogRetentionCleaner`.

### `OperationalFeatureModule` (optional)

An `IFeatureModule` implementation mirroring `CacheFeatureModule`, for hosts that already use
`PowerCSharp.Features` and want Operational's enable/disable flag resolved through the same
composite provider chain (code override → custom flag provider → environment variable →
appsettings → default) as Cache and Sanitization. `FeatureKey = "Operational"`, `Order => 0` —
registered ahead of leaf features that may want to log/capture during their own startup.

---

## 3. Two-Layer Gating

| | No package ref | Package ref + `Enabled: false` | Package ref + `Enabled: true` |
|---|---|---|---|
| **DI-resolved `IDiagnosticsService`/`IIssueManager`** | Absent — no code, no deps | NoOp floors (safe-off) | Real `DiagnosticsService`/`IssueManager` |
| **`EventLogWriter`/`DiagnosticsLoggerProvider`/`RetryPolicyProvider`** | Absent | Not registered — no background task, no custom logger | Registered; `EventLogWriter` still inert unless `LogsBasePath` is set |

---

## 4. Host Integration — Full Example

```csharp
// Program.cs — standalone (no PowerCSharp.Features dependency required)
builder.Services.AddOperational(builder.Configuration);
// ...
var app = builder.Build();
app.UseOperational();
```

```csharp
// Program.cs — via PowerCSharp.Features auto-discovery
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.ScanAssemblies(typeof(OperationalFeatureModule).Assembly);
});
var app = builder.Build();
app.UsePowerFeatures();
app.UseOperational(); // still call this to start EventLogWriter + retention cleanup
```

```csharp
using PowerCSharp.Operational.Abstractions;

public class OrderController(IDiagnosticsService diagnostics, IIssueManager issues, IRetryPolicyProvider retry)
{
    public async Task<IActionResult> Get(int id)
    {
        diagnostics.AddTrace($"Looking up order {id}");

        try
        {
            var pipeline = retry.GetPipeline();
            var response = await pipeline.ExecuteAsync(async _ => await _httpClient.GetAsync($"/orders/{id}"));
            return Ok(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            issues.CaptureException(ex, new { orderId = id });
            throw;
        }
    }
}
```

```json
{
  "PowerFeatures": {
    "Operational": {
      "Enabled": true,
      "LogsBasePath": "C:\\Logs\\MyApp",
      "LogsRetentionDays": 30,
      "AppName": "MyApp",
      "DefaultLogLevel": "Warning",
      "DefaultHttpMaxAttempts": 2,
      "DefaultMethodMaxAttempts": 2
    }
  }
}
```

---

## 5. Known v1 Gaps

- **Correlation id.** No PowerCSharp-wide correlation-id abstraction exists yet.
  `IssueManager.CaptureException`, `DiagnosticsLogger.ForwardToEventViewer`, and
  `EventLogWriter.Enqueue` each mark a `// TODO` at the point a correlation id would attach; today
  `EventLogWriter` falls back to `HttpContext.TraceIdentifier` (or a fresh GUID with no context).
- **Windows Event Viewer.** `IEventViewerService` ships only `NoOpEventViewerService`. A
  `PowerCSharp.Operational.WinEventLog` provider package is a candidate future addition — see
  [`PowerCSharp.Operational.Architecture.md`](PowerCSharp.Operational.Architecture.md).
- **AOP/aspect-based capture and third-party issue-tracking providers** (e.g. a
  `PowerCSharp.Operational.Sentry` package) were explicitly excluded from v1 scope.

---

## 6. Related Documents

- [`PowerCSharp.Operational.Architecture.md`](PowerCSharp.Operational.Architecture.md) — Design rationale, decision log, and open risks
- [`PowerCSharp.Features.Architecture.md`](PowerCSharp.Features.Architecture.md) — Two-tier design, dependency topology
- [`PowerCSharp.Feature.Sanitization.md`](PowerCSharp.Feature.Sanitization.md) — The sanitization engine Operational delegates masking/sensitive-data filtering to
- [`EDGE_CASES_AND_SECURITY.md`](EDGE_CASES_AND_SECURITY.md) — Per-API edge-case and security notes
