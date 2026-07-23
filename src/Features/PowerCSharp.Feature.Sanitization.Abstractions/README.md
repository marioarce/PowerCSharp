# PowerCSharp.Feature.Sanitization.Abstractions

![PowerCSharp Banner](https://raw.githubusercontent.com/marioarce/PowerCSharp/0191ee12092c28ccf5a578e59977583117a3ff00/docs/images/PowerCSharp_Banner.png)

Framework-agnostic sanitization engine, contracts, and safe-off NoOp implementation for the PowerCSharp Sanitization feature. Covers log injection (CWE-117), file-path traversal (CWE-22), sensitive-data exposure (CWE-200), and regex-injection/ReDoS (CWE-400/CWE-730).

- Targets `netstandard2.0` and `net8.0`, so the engine can run on **.NET Framework** and **.NET Core** — including hot-path logging call sites that reference nothing else.
- No ASP.NET Core dependency.
- Only dependencies are `Microsoft.Extensions.Logging.Abstractions` (the `ILogger` parameter on the NoOp service) and `System.Text.Json` (used internally by the log-injection JSON-encoding strategy).
- The engine is fail-safe: sanitization methods never throw. Only `SanitizeForFilePath` and `SanitizeForRegexInjection` can reject an input (via `IsRejected` on the result), and only under strict validation.

## Contents

- `SanitizationEngine` — static, partial engine class. Callable directly with no DI/configuration required; optionally wired to host configuration via `SetConfigurationProvider`.
- `SanitizationExtensions` — string extension methods (`SanitizeForLog`, `SanitizeForFilePath`, `SanitizeForSensitiveData`, `SanitizeForRegexInjection`, and `*WithDetails` variants, plus `Mask`). The primary call-site API — no DI required.
- `ISanitizationService` — DI-facing contract wrapping the engine, sourced from host configuration.
- `ISanitizationSettingsProvider` — bridges a host's configuration/options system into the engine.
- `SanitizationSettings` — the full configurable surface for every sanitization concern.
- `SanitizationResult` / `SensitiveDataResult` — operation result models (`Unchanged` / `Modified` / `Rejected` factories).
- `SanitizationType`, `SanitizationStrategy`, `SensitiveDataDetectionStrictness` — enums.
- `NoOpSanitizationService` — safe-off floor so dependents always resolve when the feature is disabled.

## Namespaces

```csharp
using PowerCSharp.Feature.Sanitization.Abstractions;        // SanitizationEngine, SanitizationExtensions, SanitizationSettings, results
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;  // SanitizationType, SanitizationStrategy, SensitiveDataDetectionStrictness
using PowerCSharp.Feature.Sanitization.Abstractions.NoOp;   // NoOpSanitizationService
```

## Usage without DI

```csharp
using PowerCSharp.Feature.Sanitization.Abstractions;

string safeForLog = untrustedInput.SanitizeForLog();              // CWE-117
string safeForPath = untrustedSegment.SanitizeForFilePath();       // CWE-22 (throws if rejected under strict validation)
string masked = payload.SanitizeForSensitiveData();                // CWE-200
string safePattern = untrustedPattern.SanitizeForRegexInjection(); // CWE-400/CWE-730
```

Every method accepts an optional `SanitizationSettings` argument; when omitted, it uses whatever was wired via `SanitizationEngine.SetConfigurationProvider` (see `PowerCSharp.Feature.Sanitization`), or built-in defaults otherwise.

## Feature module

The Features-Framework module (options, DI wiring, ASP.NET Core integration) lives in `PowerCSharp.Feature.Sanitization`. This package has no dependency on it and works standalone.

## Details

- **Package ID:** `PowerCSharp.Feature.Sanitization.Abstractions`
- **Depends on:** `Microsoft.Extensions.Logging.Abstractions`, `System.Text.Json`
- **Target frameworks:** `netstandard2.0` and `net8.0`
