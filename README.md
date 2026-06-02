# PowerCSharp

![PowerCSharp Banner](docs/images/PowerCSharp_Banner.png)

[![PowerCSharp](https://img.shields.io/badge/PowerCSharp-v0.2.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/marioarce/PowerCSharp/workflows/CI/badge.svg)](https://github.com/marioarce/PowerCSharp/actions)
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

PowerCSharp is a comprehensive library of extension methods, utilities, and helper classes designed to enhance your C# development experience. Built by a senior C# architect with 20+ years of experience, this library provides practical, well-tested solutions for common programming challenges.

## 📦 Packages

PowerCSharp is organized into several focused packages:

- **[PowerCSharp.Core](src/PowerCSharp.Core/README.md)** - Core foundation and base classes for PowerCSharp library, including centralized interfaces and models
- **[PowerCSharp.Extensions](src/PowerCSharp.Extensions/README.md)** - Cross-platform extension methods for collections, HTTP, LINQ, JSON, XML, objects, types, streams, and strings
- **[PowerCSharp.Extensions.AspNetCore](src/PowerCSharp.Extensions.AspNetCore/README.md)** - ASP.NET Core specific extensions for configuration and web utilities
- **[PowerCSharp.Utilities](src/PowerCSharp.Utilities/README.md)** - Utility classes for validation, file operations, and mathematics
- **[PowerCSharp.Helpers](src/PowerCSharp.Helpers/README.md)** - Specialized helpers for JSON, cryptography, and environment operations
- **[PowerCSharp.Compatibility](src/PowerCSharp.Compatibility/README.md)** - .NET Framework compatibility layer with System.Web dependencies for legacy applications

### 🏗️ Architecture

PowerCSharp follows a clean architectural pattern with **centralized interfaces** in PowerCSharp.Core:

- **All interfaces** are located in `PowerCSharp.Core.Interfaces` namespace
- **All models** are located in `PowerCSharp.Core.Models` namespace
- Clear separation of concerns with proper dependency management
- Consistent namespace organization across the entire ecosystem

## 🚀 Installation

Install individual packages via NuGet:

```bash
dotnet add package PowerCSharp.Core
dotnet add package PowerCSharp.Extensions
dotnet add package PowerCSharp.Extensions.AspNetCore
dotnet add package PowerCSharp.Utilities
dotnet add package PowerCSharp.Helpers
dotnet add package PowerCSharp.Compatibility
```

Or install the complete suite:

```bash
dotnet add package PowerCSharp.Core
dotnet add package PowerCSharp.Extensions
dotnet add package PowerCSharp.Extensions.AspNetCore
dotnet add package PowerCSharp.Utilities
dotnet add package PowerCSharp.Helpers
dotnet add package PowerCSharp.Compatibility
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

- **Modern .NET**: .NET 8.0
- **Cross-platform**: .NET Standard 2.0 (PowerCSharp.Core, Extensions, Helpers, Utilities)
- **.NET Framework**: 4.6.2, 4.7.2, 4.8 (via PowerCSharp.Compatibility package)
- **ASP.NET Core**: .NET 8.0 (PowerCSharp.Extensions.AspNetCore package)

## 🧪 Testing

All PowerCSharp packages include comprehensive unit tests. Run tests with:

```bash
dotnet test
```

## 📚 Documentation

### Package-Specific Documentation
- **[PowerCSharp.Core](src/PowerCSharp.Core/README.md)** - Core interfaces and architecture
- **[PowerCSharp.Extensions](src/PowerCSharp.Extensions/README.md)** - Cross-platform extension methods reference  
- **[PowerCSharp.Extensions.AspNetCore](src/PowerCSharp.Extensions.AspNetCore/README.md)** - ASP.NET Core specific extensions
- **[PowerCSharp.Utilities](src/PowerCSharp.Utilities/README.md)** - Utility classes guide
- **[PowerCSharp.Helpers](src/PowerCSharp.Helpers/README.md)** - Specialized helpers reference
- **[PowerCSharp.Compatibility](src/PowerCSharp.Compatibility/README.md)** - .NET Framework compatibility layer

### Detailed API Documentation
- **[PowerCSharp.Core API](docs/PowerCSharp.Core.md)** - Complete core API reference
- **[PowerCSharp.Extensions API](docs/PowerCSharp.Extensions.md)** - Cross-platform extensions documentation
- **[PowerCSharp.Extensions.AspNetCore API](docs/PowerCSharp.Extensions.AspNetCore.md)** - ASP.NET Core extensions API
- **[PowerCSharp.Utilities API](docs/PowerCSharp.Utilities.md)** - Utilities API reference
- **[PowerCSharp.Helpers API](docs/PowerCSharp.Helpers.md)** - Helpers API documentation
- **[PowerCSharp.Compatibility API](docs/PowerCSharp.Compatibility.md)** - .NET Framework compatibility API
- **[Extensions API Reference](docs/PowerCSharp.Extensions-API.md)** - Complete extensions catalog

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
