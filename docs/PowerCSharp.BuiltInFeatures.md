# PowerCSharp.BuiltInFeatures — API Reference

> Bundle of lightweight, runtime-flag-toggled ASP.NET Core capabilities. Toggled via `PowerFeatures:<Key>:Enabled`. Any built-in can be disabled and replaced by a custom Pluggable Feature.

---

## Package Information

| Property | Value |
|---|---|
| **Package ID** | `PowerCSharp.BuiltInFeatures` |
| **Version** | `$(PowerCSharpFeaturesVersion)` |
| **Target Framework** | `net8.0` |
| **Dependencies** | `PowerCSharp.Features`, `Microsoft.AspNetCore.App` |
| **NuGet** | [PowerCSharp.BuiltInFeatures](https://www.nuget.org/packages/PowerCSharp.BuiltInFeatures) |

---

## 1. What Are Built-in Features?

Built-in Features (Group 1) are low-complexity ASP.NET Core / pipeline capabilities that:

- Ship together in a single bundle package.
- Carry **no third-party dependencies** — only ASP.NET Core framework references.
- Are gated by **runtime flag only** (no package-reference gating).
- Can each be **disabled and replaced** by a custom Pluggable Feature that provides the same `FeatureKey`.

---

## 2. Host Integration

### Step 1 — Register

```csharp
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.AddBuiltInFeatures(); // opts the bundle into auto-discovery
});
```

`AddBuiltInFeatures()` is an extension on `PowerFeaturesOptions` that calls `ScanAssemblies(typeof(CorsFeatureModule).Assembly)`.

### Step 2 — Apply

```csharp
var app = builder.Build();
app.UsePowerFeatures(); // applies enabled built-ins' middleware in Order
```

---

## 3. Included Built-in Features

### CORS (`PowerFeatures:Cors`)

| Property | Value |
|---|---|
| **Feature Key** | `Cors` |
| **Module** | `CorsFeatureModule` |
| **Order** | `10` |
| **Default** | Disabled |

Registers and applies a named CORS policy using `IApplicationBuilder.UseCors`. When disabled, no policy is registered and no middleware is added.

#### Configuration

```json
{
  "PowerFeatures": {
    "Cors": {
      "Enabled": true,
      "AllowedOrigins": [ "https://example.com", "https://app.example.com" ],
      "AllowedMethods": [ "GET", "POST", "PUT", "DELETE" ],
      "AllowedHeaders": [ "Authorization", "Content-Type" ],
      "AllowCredentials": false
    }
  }
}
```

#### Environment variable override

```
POWERFEATURES__CORS__ENABLED=true
```

---

## 4. `BuiltInFeaturesOptionsExtensions`

```csharp
public static PowerFeaturesOptions AddBuiltInFeatures(this PowerFeaturesOptions options)
```

Convenience method that opts the `PowerCSharp.BuiltInFeatures` assembly into auto-discovery. Equivalent to:

```csharp
options.ScanAssemblies(typeof(CorsFeatureModule).Assembly);
```

---

## 5. Replacing a Built-in with a Custom Feature

Disable the built-in via flag and register your own Pluggable Feature with the same `FeatureKey`:

```json
{
  "PowerFeatures": {
    "Cors": { "Enabled": false }
  }
}
```

```csharp
// Your custom CORS module
public class CustomCorsFeatureModule : IFeatureModule
{
    public string FeatureKey => "Cors";
    public int Order => 10;
    // ...
}

builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.AddBuiltInFeatures();
    options.AddModule(new CustomCorsFeatureModule()); // explicit registration
});
```

The built-in CORS module will be skipped (flag off); your custom module runs instead.

---

## 6. Roadmap — Planned Built-in Features

The following are planned for future minor releases:

| Feature Key | Description |
|---|---|
| `CorrelationId` | Injects/reads a correlation ID header for distributed tracing |
| `SecurityHeaders` | Adds security response headers (CSP, HSTS, X-Frame-Options, etc.) |
| `ExceptionHandling` | Global exception handler middleware with structured error responses |
| `Sanitization` | Request/response body sanitization |
| `HealthChecks` | Standard ASP.NET Core health check endpoint wiring |

---

## 7. Related Documents

- [`PowerCSharp.Features.md`](PowerCSharp.Features.md) — Features engine API reference
- [`PowerCSharp.Features.Architecture.md`](PowerCSharp.Features.Architecture.md) — Two-tier system design
- [`PowerCSharp.Features.Authoring-Guide.md`](PowerCSharp.Features.Authoring-Guide.md) — How to build a new Built-in Feature
- [`PowerCSharp.Features.FlagReference.md`](PowerCSharp.Features.FlagReference.md) — Flag schema and precedence rules
