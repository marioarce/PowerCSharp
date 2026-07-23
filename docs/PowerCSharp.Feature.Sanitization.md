# PowerCSharp.Feature.Sanitization — API Reference

> The Sanitization pluggable feature family: a framework-agnostic engine plus the Features-Framework module. Each package in the family versions independently under `PowerCSharpFeatureSanitizationVersion`. Migrated and de-branded from an internal reference implementation, then refactored from one large class into concern-based files (log injection, file path, sensitive data, regex injection).

---

## Package Family Overview

| Package | Role | Target Frameworks | Version |
|---|---|---|---|
| `PowerCSharp.Feature.Sanitization.Abstractions` | Engine + contracts + NoOp floor | `netstandard2.0` + `net8.0` | `$(PowerCSharpFeatureSanitizationVersion)` |
| `PowerCSharp.Feature.Sanitization` | Module + options + ASP.NET Core wiring | `net8.0` | `$(PowerCSharpFeatureSanitizationVersion)` |

### Dependency direction

```
PowerCSharp.Feature.Sanitization.Abstractions   (engine + contracts + NoOp, no ASP.NET Core dep)
          ▲
          │
PowerCSharp.Feature.Sanitization                (module + options + DI/ASP.NET Core wiring)
```

Unlike the Cache family, there is no separate provider package: the engine's behavior is fixed (four sanitization concerns, each independently toggleable via settings), so `PowerCSharp.Feature.Sanitization` registers its one real implementation directly rather than deferring to a plugged-in backend.

---

## 1. PowerCSharp.Feature.Sanitization.Abstractions

### `SanitizationEngine` (static, partial)

The core engine. Usable standalone with zero configuration — every method has safe, sensible defaults via a built-in `SanitizationSettings` instance.

```csharp
public static partial class SanitizationEngine
{
    public static SanitizationResult SanitizeForLogInjection(string? input, SanitizationSettings? settings = null);
    public static SanitizationResult SanitizeForFilePath(string? input, SanitizationSettings? settings = null);
    public static string SanitizeCorrelationIdForPath(string correlationId);
    public static SensitiveDataResult SanitizeForSensitiveData(string? input, SanitizationSettings? settings = null);
    public static SanitizationResult SanitizeForRegexInjection(string? pattern, SanitizationSettings? settings = null);

    public static void SetConfigurationProvider(Func<SanitizationSettings> provider, Action<string>? securityLogger = null);
    public static void SetSecurityEventLogger(Action<string> securityLogger);
    public static Dictionary<string, object> GetPerformanceStats();
}
```

Split by concern across files for maintainability: `SanitizationEngine.cs` (shared state, configuration wiring, performance tracking), `SanitizationEngine.LogInjection.cs`, `SanitizationEngine.FilePath.cs`, `SanitizationEngine.SensitiveData.cs`, `SanitizationEngine.RegexInjection.cs`.

Fail-safe behavior: sanitization methods never throw internally — a caught exception returns the original input unchanged (log injection, sensitive data) or a strict rejection (file path, regex injection), rather than propagating.

### `SanitizationExtensions`

The primary call-site API. No DI or configuration required.

```csharp
"user input\r\ninjected".SanitizeForLog();                    // CWE-117
"../../etc/passwd".SanitizeForFilePath();                     // CWE-22 — throws InvalidOperationException if rejected
"password=hunter2".SanitizeForSensitiveData();                 // CWE-200
"(a+)+$".SanitizeForRegexInjection();                          // CWE-400/CWE-730 — returns "" if rejected
"secret-value".Mask('*');                                      // generic masking helper
```

`*WithDetails` variants (`SanitizeForLogWithDetails`, `SanitizeForFilePathWithDetails`, `SanitizeForSensitiveDataWithDetails`, `SanitizeForRegexInjectionWithDetails`) return the full `SanitizationResult`/`SensitiveDataResult` instead of a bare string, so callers can inspect `WasModified`/`IsRejected` without a try/catch.

### Result types

#### `SanitizationResult`

```csharp
public sealed class SanitizationResult : IEquatable<SanitizationResult>
{
    public string SanitizedValue { get; }
    public bool WasModified { get; }
    public SanitizationType SanitizationType { get; }
    public TimeSpan ProcessingTime { get; }
    public bool IsRejected { get; }

    public static SanitizationResult Unchanged(string originalValue, SanitizationType sanitizationType);
    public static SanitizationResult Modified(string sanitizedValue, SanitizationType sanitizationType, TimeSpan processingTime);
    public static SanitizationResult Rejected(SanitizationType sanitizationType, TimeSpan processingTime);
}
```

A plain class rather than a C# record — the package targets `netstandard2.0`, which lacks `init`-accessor support. Value equality is implemented manually, matching the convention already used by `PowerCSharp.Feature.Cache.Abstractions.CacheResult<T>`.

#### `SensitiveDataResult`

Same shape as `SanitizationResult`, with `SanitizationType` fixed to `SensitiveData` and no `Rejected` factory (sensitive-data detection only masks; it never rejects).

### `SanitizationSettings`

The full configurable surface — every property has a secure-by-default value. Grouped by concern:

```csharp
public sealed class SanitizationSettings
{
    // Log injection
    public bool EnableLogSanitization { get; set; } = true;
    public bool SanitizeUnicodeControlChars { get; set; } = true;
    public bool PreserveTabCharacters { get; set; } = false;
    public SanitizationStrategy LogSanitizationStrategy { get; set; } = SanitizationStrategy.Remove;
    public int MaxSanitizedStringLength { get; set; } = 10000;

    // Failure / security event logging
    public bool LogSanitizationFailures { get; set; } = false;
    public LogLevel SanitizationFailureLogLevel { get; set; } = LogLevel.Warning;
    public bool LogSecurityEvents { get; set; } = false;

    // Performance monitoring
    public bool EnablePerformanceMonitoring { get; set; } = false;
    public double SlowOperationThresholdMs { get; set; } = 1.0;

    // File path (CWE-22)
    public bool EnableFilePathSanitization { get; set; } = true;
    public bool EnableParentDirectoryTraversalCheck { get; set; } = true;
    public bool UseStrictValidation { get; set; } = true;       // allowlist mode (recommended) vs. legacy pattern-stripping
    public bool ValidateWindowsReservedNames { get; set; } = true;
    public bool AllowUnicodeCharacters { get; set; } = true;
    public int MaxPathSegmentLength { get; set; } = 255;
    public string[] AllowedBaseDirectories { get; set; } = Array.Empty<string>();
    public string[] AllowedFileExtensions { get; set; } = Array.Empty<string>();

    // Sensitive data (CWE-200)
    public bool EnableSensitiveDataDetection { get; set; } = true;
    public char SensitiveDataMaskCharacter { get; set; } = '*';
    public SensitiveDataDetectionStrictness SensitiveDataDetectionStrictness { get; set; } = SensitiveDataDetectionStrictness.Medium;

    // Regex injection / ReDoS (CWE-400/CWE-730)
    public bool EnableRegexSanitization { get; set; } = true;
    public TimeSpan MaxRegexValidationTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxRegexPatternLength { get; set; } = 1000;
    public int MaxRegexComplexityScore { get; set; } = 100;
    public bool AllowUnicodeCategories { get; set; } = true;
    public bool AllowQuantifiers { get; set; } = true;
    public bool AllowNestedQuantifiers { get; set; } = false;    // the classic catastrophic-backtracking shape
    public bool AllowBackreferences { get; set; } = true;
    public bool AllowLookarounds { get; set; } = true;

    public string? CorrelationId { get; set; }                  // per-call; not bound from global config
}
```

### Enums

```csharp
public enum SanitizationType { LogInjection, FilePath, SensitiveData, RegexInjection }
public enum SanitizationStrategy { Remove, ReplaceWithSpace, HtmlEncode, UrlEncode, JsonEncode }
public enum SensitiveDataDetectionStrictness { Low, Medium, High }
```

### NoOp implementation

| Class | Interface | Behavior |
|---|---|---|
| `NoOpSanitizationService` | `ISanitizationService` | Every method returns the input unchanged (`Unchanged` factory). Safe-off floor when the feature is disabled. |

### File-path sanitization: two modes

`SanitizeForFilePath` uses **allowlist validation** (recommended, `UseStrictValidation = true` by default): rejects anything suspicious — invalid characters, Windows reserved device names, oversized path segments, disallowed extensions, traversal patterns, paths outside `AllowedBaseDirectories` — rather than trying to "clean" it. When `UseStrictValidation = false`, a legacy pattern-stripping mode is used instead: `..`, doubled separators, drive letters, and `./` references are stripped rather than rejected. Prefer allowlist mode for new code; legacy mode exists for compatibility with callers migrated from the original implementation.

### Sensitive-data masking

Detects and masks (rather than rejects) tokens, secrets, credentials, URLs, and file-system paths via a layered pattern pipeline: specific patterns first (`Bearer ...`, `sk-...`, `api_key=...`, `password=...`), then generic key-based heuristics (`password:`, `token=`, `admin=`, etc. — preserving the key name, masking only the value), then structural patterns (URLs, Windows/UNC/Unix/home paths), and finally — at `Medium` strictness or higher — any long alphanumeric run (16+ characters), to catch tokens that don't match a named pattern. `MaskValueIntelligently` keeps roughly the first and last quarter of a value visible so masked output still hints at shape/length without disclosing content.

### Regex-injection / ReDoS validation

`SanitizeForRegexInjection` never executes the untrusted pattern against untrusted input — it validates the pattern itself before it's ever used: pattern length, compile-with-timeout safety, a complexity score (nesting depth, quantifier/group counts), and configurable rejection of specific catastrophic-backtracking shapes (nested quantifiers, by default disallowed) and constructs (Unicode categories, quantifiers, backreferences, lookarounds — all independently toggleable).

---

## 2. PowerCSharp.Feature.Sanitization

### `SanitizationFeatureModule`

`IFeatureModule` implementation. Auto-discoverable (parameterless constructor).

```csharp
public const string Key = "Sanitization";   // FeatureKey
public int Order => 20;
```

`ConfigureServices` behavior — decides active vs. NoOp directly (unlike Cache, there is no provider package to defer to):

1. Binds `SanitizationFeatureOptions` from `PowerFeatures:Sanitization`.
2. If the feature flag is disabled: registers `NoOpSanitizationService` and returns.
3. If enabled: registers `SanitizationSettingsProvider` and the real `SanitizationService`.

`ConfigurePipeline` behavior — contributes no middleware; used solely to bridge the DI-resolved `ISanitizationSettingsProvider` into the static `SanitizationEngine` once the application's `IServiceProvider` is built, via `SanitizationEngineServiceProviderExtensions.ConfigureSanitizationEngine`.

### `SanitizationFeatureOptions` (inherits `FeatureOptionsBase`)

Mirrors the full `SanitizationSettings` surface (minus `CorrelationId`, which is per-call rather than global configuration) so every sanitization concern is configurable via `appsettings.json`.

### `SanitizationService` / `SanitizationSettingsProvider`

```csharp
public sealed class SanitizationSettingsProvider : ISanitizationSettingsProvider
{
    public SanitizationSettingsProvider(IOptionsMonitor<SanitizationFeatureOptions> options);
    public SanitizationSettings GetCurrentSettings();
}

public sealed class SanitizationService : ISanitizationService
{
    public SanitizationService(ISanitizationSettingsProvider settingsProvider);
    // delegates every method to SanitizationEngine, sourcing settings from settingsProvider
}
```

`SanitizationService` depends on `ISanitizationSettingsProvider` rather than options directly, so the options-to-settings mapping lives in exactly one place. `SanitizationSettingsProvider` deliberately takes no `ILogger` dependency — the engine's security-event logging path can call back into settings resolution, and a logger dependency there would risk a recursive logging loop.

### `SanitizationFeatureExtensions.AddSanitizationFeature` (explicit extension)

```csharp
IServiceCollection AddSanitizationFeature(
    this IServiceCollection services,
    IConfiguration configuration)
```

Registers the real `SanitizationService` directly — not a NoOp floor. This is a deliberate departure from `AddCacheFeature`'s pattern: Cache always registers a NoOp floor because a *separate* provider package (e.g. BitFaster) supplies the real implementation via a plain `AddSingleton` that overrides it. Sanitization has no such split, so explicit opt-in here means the caller wants sanitization active.

### `SanitizationEngineServiceProviderExtensions.ConfigureSanitizationEngine`

```csharp
IServiceProvider ConfigureSanitizationEngine(this IServiceProvider serviceProvider)
```

Resolves `ISanitizationSettingsProvider` from the given provider, if registered, and wires it into the static engine via `SanitizationEngine.SetConfigurationProvider`. A no-op (returns the same provider unchanged) when the feature is disabled or no settings provider is registered. Called automatically by `SanitizationFeatureModule.ConfigurePipeline` under auto-discovery; call it explicitly after `AddSanitizationFeature` when not using the Features engine's pipeline.

### Configuration

```json
{
  "PowerFeatures": {
    "Sanitization": {
      "Enabled": true,
      "EnableLogSanitization": true,
      "LogSanitizationStrategy": "Remove",
      "EnableFilePathSanitization": true,
      "UseStrictValidation": true,
      "EnableSensitiveDataDetection": true,
      "SensitiveDataDetectionStrictness": "Medium",
      "EnableRegexSanitization": true,
      "AllowNestedQuantifiers": false
    }
  }
}
```

---

## 3. Two-Layer Gating

| | No package ref | Package ref + flag off | Package ref + flag on |
|---|---|---|---|
| **DI-resolved `ISanitizationService`** | Absent — no code, no deps | `NoOpSanitizationService` (safe-off) | Real `SanitizationService` |
| **Static `SanitizationEngine` extension methods** | N/A (Abstractions not referenced) | Active regardless of the flag, using built-in defaults unless a host explicitly wires disabled settings | Active, reflecting bound configuration once `ConfigureSanitizationEngine` has run |

The static engine's extension-method API (`input.SanitizeForLog()`) is usable as a pure utility with zero registration, independent of the Features Framework flag — the flag only gates the *DI-resolved* `ISanitizationService`, and (indirectly) whether the engine gets wired to reflect host configuration versus its built-in defaults.

---

## 4. Host Integration — Full Example

```csharp
// Program.cs

// Option A: auto-discovery via engine scan
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.ScanAssemblies(typeof(SanitizationFeatureModule).Assembly);
});

var app = builder.Build();
app.UsePowerFeatures(); // also wires the static engine

// Option B: explicit registration (no reflection)
builder.Services.AddSanitizationFeature(builder.Configuration);
// ...
var app2 = builder.Build();
app2.Services.ConfigureSanitizationEngine();
```

```csharp
// In a service/controller
public class MyService(ISanitizationService sanitizer, ILogger<MyService> logger)
{
    public void HandleUserInput(string untrustedInput)
    {
        var result = sanitizer.SanitizeForLogInjection(untrustedInput);
        logger.LogInformation("Received: {Input}", result.SanitizedValue);
    }
}

// Or, without DI, anywhere PowerCSharp.Feature.Sanitization.Abstractions is referenced:
logger.LogInformation("Received: {Input}", untrustedInput.SanitizeForLog());
```

---

## 5. Related Documents

- [`PowerCSharp.Features.Architecture.md`](PowerCSharp.Features.Architecture.md) — Two-tier design, dependency topology
- [`PowerCSharp.Features.Authoring-Guide.md`](PowerCSharp.Features.Authoring-Guide.md) — How to build a new pluggable feature
- [`PowerCSharp.Feature.Cache.md`](PowerCSharp.Feature.Cache.md) — The Cache family, for contrast with the provider-package pattern
- [`EDGE_CASES_AND_SECURITY.md`](EDGE_CASES_AND_SECURITY.md) — Per-API edge-case and security notes
