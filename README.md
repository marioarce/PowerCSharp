# PowerCSharp

[![PowerCSharp](https://img.shields.io/badge/PowerCSharp-v1.0.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/marioarce/PowerCSharp/workflows/CI/badge.svg)](https://github.com/marioarce/PowerCSharp/actions)
[![codecov](https://codecov.io/gh/marioarce/PowerCSharp/branch/main/graph/badge.svg)](https://codecov.io/gh/marioarce/PowerCSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Core.svg)](https://www.nuget.org/packages/PowerCSharp.Core)
[![Code Quality](https://img.shields.io/badge/code%20quality-A%2B-brightgreen)](https://github.com/marioarce/PowerCSharp)

Enhanced C# extension methods and utilities for .NET developers

[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Core.svg)](https://www.nuget.org/packages/PowerCSharp.Core)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Extensions.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Utilities.svg)](https://www.nuget.org/packages/PowerCSharp.Utilities)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Helpers.svg)](https://www.nuget.org/packages/PowerCSharp.Helpers)

PowerCSharp is a comprehensive library of extension methods, utilities, and helper classes designed to enhance your C# development experience. Built by a senior C# architect with 20+ years of experience, this library provides practical, well-tested solutions for common programming challenges.

## 📦 Packages

PowerCSharp is organized into several focused packages:

- **PowerCSharp.Core** - Core string manipulation and validation extensions
- **PowerCSharp.Extensions** - Comprehensive extension methods for collections, HTTP, LINQ, JSON, XML, objects, types, streams, and configuration
- **PowerCSharp.Utilities** - Utility classes for validation, file operations, and mathematics
- **PowerCSharp.Helpers** - Specialized helpers for JSON, cryptography, and environment operations

## 🚀 Installation

Install individual packages via NuGet:

```bash
dotnet add package PowerCSharp.Core
dotnet add package PowerCSharp.Extensions
dotnet add package PowerCSharp.Utilities
dotnet add package PowerCSharp.Helpers
```

Or install the complete suite:

```bash
dotnet add package PowerCSharp.Core
dotnet add package PowerCSharp.Extensions
dotnet add package PowerCSharp.Utilities
dotnet add package PowerCSharp.Helpers
```

## 💡 Usage Examples

### String Extensions (PowerCSharp.Core & PowerCSharp.Extensions)

```csharp
using PowerCSharp.Core;
using PowerCSharp.Extensions;

string text = "hello world";
bool isEmpty = text.IsNullOrWhiteSpace(); // false
string title = text.ToTitleCase(); // "Hello World"
string safe = text.SafeSubstring(0, 5); // "hello"

// New string utilities
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

### HTTP & Network Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;
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

### Configuration Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder().Build();
var options = configuration.GetOptions<MyAppOptions>("MyApp"); // Reads from "MyApp" section
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

- .NET 8.0
- .NET Standard 2.0 (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

## 🧪 Testing

All PowerCSharp packages include comprehensive unit tests. Run tests with:

```bash
dotnet test
```

## 📚 Documentation

- [API Documentation](docs/) - Complete API reference
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
