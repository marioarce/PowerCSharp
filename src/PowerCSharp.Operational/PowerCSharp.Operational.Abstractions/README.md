# PowerCSharp.Operational.Abstractions

![PowerCSharp Banner](https://raw.githubusercontent.com/marioarce/PowerCSharp/0191ee12092c28ccf5a578e59977583117a3ff00/docs/images/PowerCSharp_Banner.png)

Framework-agnostic contracts and NoOp floors for **PowerCSharp Operational** — a cross-cutting
mechanism for issue capture, diagnostics, and logging, usable in any application architecture
(Clean, Onion, Hexagonal, or monolith).

- Targets `netstandard2.0` and `net8.0`, so hosts on **.NET Framework** and **.NET Core** can both
  depend on the contracts.
- No ASP.NET Core dependency.
- Only dependency is `Microsoft.Extensions.Logging.Abstractions`.
- Fully usable with **zero DI registration**: if nothing is wired up, `PowerCSharp.Operational`'s
  `AddOperational()` registers the NoOp floors in this package, and the host application behaves
  exactly as if Operational were never referenced — connect or disconnect Operational at any time
  without risk of crashing the app.

## Contents

- `IDiagnosticsService` — in-app diagnostics: trace/breadcrumb/exception/error capture and a
  filtered, sanitized snapshot for troubleshooting.
- `IIssueManager` — centralized exception/error/breadcrumb capture, forwarded to diagnostics (and,
  optionally, a third-party provider package in a later phase).
- `IEventViewerService` — pluggable hook for forwarding log entries to a platform event log (e.g.
  Windows Event Viewer). No platform-specific code lives in this package.
- `OperationalOptions` — configuration bound from `PowerFeatures:Operational`.
- `DiagnosticEvent`, `DiagnosticsPayload`, `EventViewerLogEntry`, `SensitiveDataAttribute` — models.
- `TraceLevel`, `BreadcrumbLevel`, `DiagnosticEventType` — enums.
- `NoOpDiagnosticsService`, `NoOpIssueManager`, `NoOpEventViewerService` — safe-off floors.

## Namespaces

```csharp
using PowerCSharp.Operational.Abstractions;         // IDiagnosticsService, IIssueManager, IEventViewerService, OperationalOptions
using PowerCSharp.Operational.Abstractions.Enums;   // TraceLevel, BreadcrumbLevel, DiagnosticEventType
using PowerCSharp.Operational.Abstractions.Models;  // DiagnosticEvent, DiagnosticsPayload, EventViewerLogEntry, SensitiveDataAttribute
using PowerCSharp.Operational.Abstractions.NoOp;    // NoOpDiagnosticsService, NoOpIssueManager, NoOpEventViewerService
```

## Where Operational sits

Unlike the Cache/Sanitization Features (optional leaf capabilities an app opts into),
`PowerCSharp.Operational` is cross-cutting plumbing other code depends on — so it sits **beside**
the Features Framework rather than inside it. It optionally integrates with
`PowerCSharp.Features` for config-driven enable/disable, but never requires it. See
`docs/PowerCSharp.Operational.Architecture.md` for the full reasoning.

## Details

- **Package ID:** `PowerCSharp.Operational.Abstractions`
- **Depends on:** `Microsoft.Extensions.Logging.Abstractions`
- **Target frameworks:** `netstandard2.0` and `net8.0`
- **Real implementation:** `PowerCSharp.Operational` (ASP.NET Core, `net8.0`)
