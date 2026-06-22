# PowerCSharp

![PowerCSharp Banner](docs/images/PowerCSharp_Banner.png)

[![PowerCSharp](https://img.shields.io/badge/PowerCSharp-v2.0.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/marioarce/PowerCSharp/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/marioarce/PowerCSharp/actions)
[![codecov](https://codecov.io/gh/marioarce/PowerCSharp/branch/main/graph/badge.svg)](https://codecov.io/gh/marioarce/PowerCSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Core.svg)](https://www.nuget.org/packages/PowerCSharp.Core)
[![Code Quality](https://img.shields.io/badge/code%20quality-A%2B-brightgreen)](https://github.com/marioarce/PowerCSharp)

Enhanced C# extension methods and utilities for .NET developers

[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Core.svg)](https://www.nuget.org/packages/PowerCSharp.Core)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Extensions.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Extensions.AspNetCore.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions.AspNetCore)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Utilities.svg)](https://www.nuget.org/packages/PowerCSharp.Utilities)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Helpers.svg)](https://www.nuget.org/packages/PowerCSharp.Helpers)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Compatibility.svg)](https://www.nuget.org/packages/PowerCSharp.Compatibility)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Features.Abstractions.svg)](https://www.nuget.org/packages/PowerCSharp.Features.Abstractions)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Features.svg)](https://www.nuget.org/packages/PowerCSharp.Features)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.BuiltInFeatures.svg)](https://www.nuget.org/packages/PowerCSharp.BuiltInFeatures)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Feature.Cache.svg)](https://www.nuget.org/packages/PowerCSharp.Feature.Cache)

PowerCSharp is a comprehensive library of extension methods, utilities, and helper classes designed to enhance your C# development experience. Built by a senior C# architect with 20+ years of experience, this library provides practical, well-tested solutions for common programming challenges.

**PowerCSharp v2.0.0 — Features Framework Release**

### What's New in v2.0.0:
- **Features Framework**: Brand-new `PowerCSharp.Features` engine — hybrid auto-scan + explicit module discovery, composite flag resolution (config → env vars → overrides), DI orchestration, opt-in diagnostics endpoint
- **Built-in Features**: `PowerCSharp.BuiltInFeatures` bundle — runtime-flag-toggled ASP.NET Core capabilities (CORS), toggled via `PowerFeatures:<Key>:Enabled`
- **Cache Feature Family**: `PowerCSharp.Feature.Cache` (module + options), `PowerCSharp.Feature.Cache.Abstractions` (contracts + NoOp, `netstandard2.0` + `net8.0`), `PowerCSharp.Feature.Cache.BitFaster` (BitFaster-backed LRU), `PowerCSharp.Feature.Cache.Disk` (disk-backed LRU with cross-process locking)
- **EditorConfig**: Comprehensive coding standards applied across the entire codebase
- **Directory Extensions**: `TrySafeDelete` and related safe I/O helpers
- **Code Quality**: Nullable annotations, member ordering, and namespace cleanup throughout

## 📦 Packages

PowerCSharp is organized into focused, independently versioned packages.

### Core Libraries (`v2.0.0`)

- **[PowerCSharp.Core](src/PowerCSharp.Core/README.md)** - Core foundation and base classes, including centralized interfaces and models
- **[PowerCSharp.Extensions](src/PowerCSharp.Extensions/README.md)** - Cross-platform extension methods for collections, HTTP, LINQ, JSON, XML, objects, types, streams, and strings
- **[PowerCSharp.Extensions.AspNetCore](src/PowerCSharp.Extensions.AspNetCore/README.md)** - ASP.NET Core specific extensions for configuration and web utilities
- **[PowerCSharp.Utilities](src/PowerCSharp.Utilities/README.md)** - Utility classes for validation, file operations, and mathematics
- **[PowerCSharp.Helpers](src/PowerCSharp.Helpers/README.md)** - Specialized helpers for JSON, cryptography, and environment operations
- **[PowerCSharp.Compatibility](src/PowerCSharp.Compatibility/README.md)** - .NET Framework compatibility layer (`net462`/`net472`/`net48`)

### Features Framework (`v1.0.0`)

- **[PowerCSharp.Features.Abstractions](src/Features/PowerCSharp.Features.Abstractions/README.md)** - Contracts only: `IFeatureModule`, `IFeatureFlagProvider`, `FeatureOptionsBase`, `FeatureDescriptor`. Zero third-party dependencies.
- **[PowerCSharp.Features](src/Features/PowerCSharp.Features/README.md)** - The engine: assembly discovery, composite flag resolution, DI orchestration, feature registry, and diagnostics. Entry points: `AddPowerFeatures()` / `UsePowerFeatures()`.
- **[PowerCSharp.BuiltInFeatures](src/Features/PowerCSharp.BuiltInFeatures/README.md)** - Bundle of lightweight, runtime-toggled ASP.NET Core capabilities (CORS). Toggle via `PowerFeatures:<Key>:Enabled`.

### Cache Feature Family (`v1.3.0`)

- **[PowerCSharp.Feature.Cache.Abstractions](src/Features/PowerCSharp.Feature.Cache.Abstractions/README.md)** - Cache contracts (`ICacheService`, `IDiskCacheService`, metadata types) and NoOp safe-off implementations. Targets `netstandard2.0` + `net8.0`.
- **[PowerCSharp.Feature.Cache](src/Features/PowerCSharp.Feature.Cache/README.md)** - Cache feature module, options, and `AddCacheFeature()` wiring. Pair with a provider package.
- **[PowerCSharp.Feature.Cache.BitFaster](src/Features/PowerCSharp.Feature.Cache.BitFaster/README.md)** - BitFaster-backed in-memory LRU cache. Isolates `BitFaster.Caching` dependency.
- **[PowerCSharp.Feature.Cache.Disk](src/Features/PowerCSharp.Feature.Cache.Disk/README.md)** - Disk-backed LRU cache with atomic writes, cross-process file-lock coordination, and background cleanup.

### 🏗️ Architecture

PowerCSharp follows a clean architectural pattern with **centralized interfaces** in PowerCSharp.Core:

- **All interfaces** are located in `PowerCSharp.Core.Interfaces` namespace
- **All models** are located in `PowerCSharp.Core.Models` namespace
- **Clear separation of concerns** with proper dependency management
- **Consistent namespace organization** across the entire ecosystem
- **Modular design** allowing selective package installation
- **Dependency-free core** for maximum compatibility

## 🚀 Installation

### Core libraries

```bash
dotnet add package PowerCSharp.Core
dotnet add package PowerCSharp.Extensions
dotnet add package PowerCSharp.Extensions.AspNetCore
dotnet add package PowerCSharp.Utilities
dotnet add package PowerCSharp.Helpers
dotnet add package PowerCSharp.Compatibility
```

### Features framework

```bash
dotnet add package PowerCSharp.Features.Abstractions
dotnet add package PowerCSharp.Features
dotnet add package PowerCSharp.BuiltInFeatures
```

### Cache feature (pick a provider)

```bash
dotnet add package PowerCSharp.Feature.Cache
dotnet add package PowerCSharp.Feature.Cache.BitFaster   # in-memory LRU (BitFaster)
dotnet add package PowerCSharp.Feature.Cache.Disk        # disk-backed LRU
```

## 💡 Usage Examples

### String Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

string text = "hello world";
bool isEmpty = text.IsNullOrWhiteSpace(); // false
string title = text.ToTitleCase(); // "Hello World"
string safe = text.SafeSubstring(0, 5); // "hello"

// Additional string utilities
string camel = "HelloWorld".ToCamelCase(); // "helloWorld"
string firstLower = text.FirstCharToLowerCase(); // "hello world"
string mid = text.Mid(6); // "world"
string normalized = "User Name".NormalizeKey(); // "userName"
string ascii = "café".AsAscii(); // "caf"
bool isValid = "https://example.com".IsValidUrl(); // true
```

### Collection Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

var numbers = new List<int> { 1, 2, 3, 4, 5 };
bool isEmpty = numbers.IsNullOrEmpty(); // false
var first = numbers.FirstOrDefaultSafe(-1); // 1
var page = numbers.Page(1, 2); // [1, 2]

// New collection utilities
var list = new List<string> { "keep", "remove", "keep", "remove" };
int removed = list.RemoveAll(x => x == "remove"); // 2
```

### DateTime Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

var date = DateTime.Now;
int age = date.GetAge();
bool isWeekend = date.IsWeekend();
var firstDay = date.FirstDayOfMonth();
var lastDay = date.LastDayOfMonth();
```

### HTTP & Network Extensions (PowerCSharp.Extensions.AspNetCore)

```csharp
using PowerCSharp.Extensions.AspNetCore;
using System.Net;

// HTTP Status Code utilities
HttpStatusCode status = HttpStatusCode.OK;
bool success = status.IsSuccessful(); // true
bool clientError = status.IsClientError(); // false
bool serverError = status.IsServerError(); // false
bool isRedirect = status.IsRedirect(); // false

// URI manipulation
Uri uri = new Uri("https://example.com");
Uri withParam = uri.AddParameter("search", "test"); // https://example.com?search=test

// HTTP Request cloning
using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com");
var clonedRequest = request.Clone();
var clonedAsync = await request.CloneAsync();
```

### Configuration Extensions (PowerCSharp.Extensions.AspNetCore)

```csharp
using PowerCSharp.Extensions.AspNetCore;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder().Build();
var options = configuration.GetOptions<MyAppOptions>("MyApp"); // Reads from "MyApp" section
```

### Features Framework (PowerCSharp.Features + PowerCSharp.BuiltInFeatures)

```csharp
// Program.cs
builder.Services.AddPowerFeatures(builder.Configuration, options =>
{
    options.AddBuiltInFeatures();          // opt-in the built-in bundle (CORS, etc.)
    options.ScanAssemblies(              // opt-in pluggable feature assemblies
        typeof(CacheFeatureModule).Assembly);
    options.Override("Cache", true);     // optional code-level override
    options.EnableDiagnosticsEndpoint(); // GET /power-features — off by default
});

var app = builder.Build();
app.UsePowerFeatures();
```

```json
// appsettings.json
{
  "PowerFeatures": {
    "Cors": { "Enabled": true, "AllowedOrigins": [ "https://example.com" ] },
    "Cache": { "Enabled": true, "Provider": "BitFaster", "Capacity": 1000 }
  }
}
```

Flag resolution order (highest precedence first): explicit code override → custom `IFeatureFlagProvider` → environment variables (`POWERFEATURES__<KEY>__ENABLED`) → `appsettings` → feature default.

### Cache Feature (PowerCSharp.Feature.Cache + provider)

```csharp
// Resolves to the active provider (BitFaster or Disk) or NoOp if disabled.
var cache = app.Services.GetRequiredService<ICacheService>();

await cache.SetAsync("key", myObject, TimeSpan.FromMinutes(5));
var result = await cache.GetAsync<MyObject>("key");

if (result.Hit)
    Console.WriteLine(result.Value);
```

### LINQ & Dynamic Query Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

// Dynamic expression parsing
string expression = "Age > 18 && Name.Contains('John')";
var predicate = expression.GetExpressionDelegate<Person>();

// Dynamic ordering
string orderExpression = "Name DESC, Age ASC";
var orderDelegates = orderExpression.GetOrderDelegates<Person>();

// Dynamic filtering and ordering
var filterProvider = new DynamicFilterProvider<Person>();
var orderProvider = new DynamicOrderProvider<Person>();
var filtered = people.Filter(filterProvider);
var ordered = people.Order(orderProvider);
```

### JSON & XML Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;
using System.Text.Json;

// JSON element access
JsonElement element = JsonDocument.Parse("{\"name\":\"John\"}").RootElement;
var name = element.Get("name"); // JsonElement with value "John"
var firstItem = element.Get(0); // For arrays

// Case-insensitive JSON access
bool found = element.TryGetPropertyCaseInsensitive("NAME", out var value);

// XML flattening
using System.Xml.Linq;
XElement xml = XElement.Parse("<root><child>value</child></root>");
var dict = xml.Flatten(); // Dictionary with XML structure
```

### Object & Type Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

// Object utilities
string text = "test";
text.ThrowOnNull(); // Throws if null

bool isTrue = "true".TryGetBool(out bool result); // result = true, isTrue = true
bool isFalse = "0".TryGetBool(out result); // result = false, isFalse = true

// Generic operations
var person = new Person { Name = "John", Age = 30 };
var copy = new Person();
person.CopyPropertiesTo(copy); // Copies matching properties

// Type operations
bool isDefault = default(int).IsDefault(); // true
string typeName = typeof(List<string>).GetGenericTypeName(); // "List<String>"

// Concrete type resolution
Type concreteType = typeof(IMyInterface).GetConcreteType();
```

### Stream Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes("test data"));
using var destinationStream = new MemoryStream();

await originalStream.CloneAsync(destinationStream);
// destinationStream now contains the same data as originalStream
```

### Object Hash Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

var person = new { Name = "John", Age = 30, Email = "john@example.com" };
string hash = person.ComputeHash(); // "A1B2C3D4E5F67890" (16-char hex string)

// Handles complex objects with nested properties
var complexObj = new Order 
{ 
    Id = 123, 
    Customer = new Customer { Name = "Alice" }, 
    Items = new List<Item> { new Item { Name = "Product1" } }
};
string orderHash = complexObj.ComputeHash(); // Consistent hash for caching/identification
```

### Secure Path Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

string basePath = "/var/www/uploads";
string userFile = "../../etc/passwd"; // Malicious attempt

// This will throw SecurityException due to directory traversal attempt
string safePath = PathExtensions.CombineAndValidate(basePath, userFile);

// Safe usage with valid relative paths
string validPath = PathExtensions.CombineAndValidate(basePath, "images/photo.jpg");
// Returns: "/var/www/uploads/images/photo.jpg"

// Multiple path segments
string multiPath = PathExtensions.CombineAndValidate(basePath, "documents", "2023", "report.pdf");
// Returns: "/var/www/uploads/documents/2023/report.pdf"
```


### Validation Utilities (PowerCSharp.Utilities)

```csharp
using PowerCSharp.Utilities;

bool isValidEmail = ValidationHelper.IsValidEmail("user@example.com");
bool isNumeric = ValidationHelper.IsNumeric("12345");
bool isValidUrl = ValidationHelper.IsValidUrl("https://example.com");
```

### File Utilities (PowerCSharp.Utilities)

```csharp
using PowerCSharp.Utilities;

string content = FileHelper.SafeReadAllText("file.txt");
bool success = FileHelper.SafeWriteAllText("output.txt", "Hello World");
string size = FileHelper.GetFileSize(1024 * 1024); // "1 MB"
```

### JSON Helpers (PowerCSharp.Helpers)

```csharp
using PowerCSharp.Helpers;

var obj = new { Name = "John", Age = 30 };
string json = JsonHelper.SafeSerialize(obj);
var deserialized = JsonHelper.SafeDeserialize<MyClass>(json);
string pretty = JsonHelper.PrettyPrint(json);
```

### Crypto Helpers (PowerCSharp.Helpers)

```csharp
using PowerCSharp.Helpers;

string sha256 = CryptoHelper.ComputeSHA256("password");
string md5 = CryptoHelper.ComputeMD5("data");
string random = CryptoHelper.GenerateRandomString(10);
```

## 🎯 Target Frameworks

- **Modern .NET**: .NET 8.0 — core libraries, Features engine, BuiltInFeatures, Cache feature modules
- **.NET Standard 2.0 + .NET 8.0**: `PowerCSharp.Features.Abstractions`, `PowerCSharp.Feature.Cache.Abstractions`, `PowerCSharp.Feature.Cache.BitFaster` — usable from .NET Framework and .NET Core
- **.NET Framework**: 4.6.2, 4.7.2, 4.8 — via `PowerCSharp.Compatibility`
- **ASP.NET Core**: .NET 8.0 — `PowerCSharp.Extensions.AspNetCore`, Features engine, BuiltInFeatures

## 🧪 Testing

All PowerCSharp packages include comprehensive unit tests. Run tests with:

```bash
dotnet test
```

## 📚 Documentation

### Package-Specific Documentation

**Core libraries**
- **[PowerCSharp.Core](src/PowerCSharp.Core/README.md)** - Core interfaces and architecture
- **[PowerCSharp.Extensions](src/PowerCSharp.Extensions/README.md)** - Cross-platform extension methods reference
- **[PowerCSharp.Extensions.AspNetCore](src/PowerCSharp.Extensions.AspNetCore/README.md)** - ASP.NET Core specific extensions
- **[PowerCSharp.Utilities](src/PowerCSharp.Utilities/README.md)** - Utility classes guide
- **[PowerCSharp.Helpers](src/PowerCSharp.Helpers/README.md)** - Specialized helpers reference
- **[PowerCSharp.Compatibility](src/PowerCSharp.Compatibility/README.md)** - .NET Framework compatibility layer

**Features framework**
- **[PowerCSharp.Features.Abstractions](src/Features/PowerCSharp.Features.Abstractions/README.md)** - Contracts reference
- **[PowerCSharp.Features](src/Features/PowerCSharp.Features/README.md)** - Engine integration guide
- **[PowerCSharp.BuiltInFeatures](src/Features/PowerCSharp.BuiltInFeatures/README.md)** - Built-in bundle guide

**Cache feature family**
- **[PowerCSharp.Feature.Cache.Abstractions](src/Features/PowerCSharp.Feature.Cache.Abstractions/README.md)** - Cache contracts reference
- **[PowerCSharp.Feature.Cache](src/Features/PowerCSharp.Feature.Cache/README.md)** - Cache module guide
- **[PowerCSharp.Feature.Cache.BitFaster](src/Features/PowerCSharp.Feature.Cache.BitFaster/README.md)** - BitFaster provider guide
- **[PowerCSharp.Feature.Cache.Disk](src/Features/PowerCSharp.Feature.Cache.Disk/README.md)** - Disk provider guide

### Detailed API Documentation
- **[PowerCSharp.Core API](docs/PowerCSharp.Core.md)** - Complete core API reference
- **[PowerCSharp.Extensions API](docs/PowerCSharp.Extensions.md)** - Cross-platform extensions documentation
- **[PowerCSharp.Extensions.AspNetCore API](docs/PowerCSharp.Extensions.AspNetCore.md)** - ASP.NET Core extensions API
- **[PowerCSharp.Utilities API](docs/PowerCSharp.Utilities.md)** - Utilities API reference
- **[PowerCSharp.Helpers API](docs/PowerCSharp.Helpers.md)** - Helpers API documentation
- **[PowerCSharp.Compatibility API](docs/PowerCSharp.Compatibility.md)** - .NET Framework compatibility API
- **[Extensions API Reference](docs/PowerCSharp.Extensions-API.md)** - Complete extensions catalog
- **[Features Architecture](docs/PowerCSharp.Features.Architecture.md)** - Features system design
- **[Features Authoring Guide](docs/PowerCSharp.Features.Authoring-Guide.md)** - How to build a new feature
- **[Features Flag Reference](docs/PowerCSharp.Features.FlagReference.md)** - Flag schema and provider precedence
- **[Cache Feature API](docs/PowerCSharp.Feature.Cache.md)** - Cache family API reference

### Development Documentation
- [Examples and Samples](samples/) - Working code examples
- [Contributing Guide](CONTRIBUTING.md) - How to contribute
- [Security Policy](SECURITY.md) - Security information
- [Code of Conduct](CODE_OF_CONDUCT.md) - Community guidelines
- [Changelog](CHANGELOG.md) - Version history
- [Workflow Documentation](docs/WORKFLOW.md) - Development workflow

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with passion by [Mario Arce](https://github.com/marioarce)
- Inspired by 20+ years of C# development experience
- Community feedback and contributions

## 📞 Support

- 🐛 [Report Issues](https://github.com/marioarce/PowerCSharp/issues)
- 💡 [Feature Requests](https://github.com/marioarce/PowerCSharp/discussions)
- 📧 [Email Support](mailto:support@example.com)

---

**PowerCSharp** - Making C# development more powerful, one extension at a time! 🚀
