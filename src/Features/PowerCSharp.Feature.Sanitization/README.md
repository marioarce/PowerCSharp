# PowerCSharp.Feature.Sanitization

![PowerCSharp Banner](https://raw.githubusercontent.com/marioarce/PowerCSharp/0191ee12092c28ccf5a578e59977583117a3ff00/docs/images/PowerCSharp_Banner.png)

Sanitization feature module, options, and DI/ASP.NET Core wiring for the PowerCSharp Sanitization feature.

Pair this package with `PowerCSharp.Feature.Sanitization.Abstractions` (engine + contracts + NoOp). Unlike the Cache feature family, Sanitization has no swappable-backend provider package — this module registers the concrete implementation directly.

## Contents

- **`SanitizationFeatureOptions`** — options bound from `PowerFeatures:Sanitization`, mirroring the full `SanitizationSettings` surface.
- **`SanitizationFeatureModule`** — auto-discoverable module; registers the real service when enabled, `NoOpSanitizationService` when disabled.
- **`SanitizationService`** — the DI-facing `ISanitizationService` implementation, delegating to the engine.
- **`SanitizationSettingsProvider`** — bridges bound options into `SanitizationSettings` for the engine.
- **`SanitizationFeatureExtensions.AddSanitizationFeature`** — explicit registration extension; registers the real service directly (no NoOp floor, since there is no separate provider package to defer to).
- **`SanitizationEngineServiceProviderExtensions.ConfigureSanitizationEngine`** — wires the DI-resolved settings provider into the static `SanitizationEngine`, so extension-method call sites (`input.SanitizeForLog()`) also reflect host configuration.

## Namespaces

- `PowerCSharp.Feature.Sanitization` — options, module, service, and extension methods.

> The engine, contracts, and NoOp implementation live in `PowerCSharp.Feature.Sanitization.Abstractions`.

## Usage

### Auto-discovery (ASP.NET Core)

```csharp
using PowerCSharp.Feature.Sanitization;

builder.Services.AddPowerFeatures(builder.Configuration, o =>
    o.ScanAssemblies(typeof(SanitizationFeatureModule).Assembly));

var app = builder.Build();
app.UsePowerFeatures(); // also wires the static engine via ConfigurePipeline
```

### Explicit registration

```csharp
using PowerCSharp.Feature.Sanitization;

builder.Services.AddSanitizationFeature(builder.Configuration);

var app = builder.Build();
app.Services.ConfigureSanitizationEngine(); // wire the static engine explicitly
```

```json
{
  "PowerFeatures": {
    "Sanitization": {
      "Enabled": true,
      "EnableLogSanitization": true,
      "EnableFilePathSanitization": true,
      "EnableSensitiveDataDetection": true,
      "EnableRegexSanitization": true
    }
  }
}
```

Then resolve `ISanitizationService` from DI, or call the extension methods in `PowerCSharp.Feature.Sanitization.Abstractions` directly — both reflect the same configuration once `ConfigureSanitizationEngine` has run.

## Details

- **Package ID:** `PowerCSharp.Feature.Sanitization`
- **Depends on:** `PowerCSharp.Features.Abstractions` + `PowerCSharp.Feature.Sanitization.Abstractions`
- **Target framework:** `net8.0` (requires ASP.NET Core host)
- See: `docs/PowerCSharp.Features.Authoring-Guide.md`, `docs/PowerCSharp.Feature.Sanitization.md`
