# PowerCSharp.BuiltInFeatures

![PowerCSharp Banner](https://raw.githubusercontent.com/marioarce/PowerCSharp/0191ee12092c28ccf5a578e59977583117a3ff00/docs/images/PowerCSharp_Banner.png)

Bundle of lightweight, runtime-flag-toggled ASP.NET Core capabilities. Toggle each via
`PowerFeatures:<Key>:Enabled`. Any built-in can be disabled and replaced by a custom Pluggable Feature.

## Included features

- **CORS** (`PowerFeatures:Cors`) — registers and applies a named CORS policy.

## Usage

```csharp
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.AddBuiltInFeatures(); // opt the bundle in to auto-discovery
});

var app = builder.Build();
app.UsePowerFeatures();
```

```json
{
  "PowerFeatures": {
    "Cors": { "Enabled": true, "AllowedOrigins": [ "https://example.com" ] }
  }
}
```

## Details

- **Package ID:** `PowerCSharp.BuiltInFeatures`
- **Depends on:** `PowerCSharp.Features` + `Microsoft.AspNetCore.App`
- See: `docs/PowerCSharp.Features.Authoring-Guide.md`
