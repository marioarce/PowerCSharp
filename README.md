# PowerCSharp

Enhanced C# extension methods and utilities for .NET developers

[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Core.svg)](https://www.nuget.org/packages/PowerCSharp.Core)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Extensions.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Utilities.svg)](https://www.nuget.org/packages/PowerCSharp.Utilities)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Helpers.svg)](https://www.nuget.org/packages/PowerCSharp.Helpers)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

PowerCSharp is a comprehensive library of extension methods, utilities, and helper classes designed to enhance your C# development experience. Built by a senior C# architect with 20+ years of experience, this library provides practical, well-tested solutions for common programming challenges.

## 📦 Packages

PowerCSharp is organized into several focused packages:

- **PowerCSharp.Core** - Core string manipulation and validation extensions
- **PowerCSharp.Extensions** - Extension methods for collections, dates, and common .NET types
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

### String Extensions (PowerCSharp.Core)

```csharp
using PowerCSharp.Core;

string text = "hello world";
bool isEmpty = text.IsNullOrWhiteSpace(); // false
string title = text.ToTitleCase(); // "Hello World"
string safe = text.SafeSubstring(0, 5); // "hello"
```

### Collection Extensions (PowerCSharp.Extensions)

```csharp
using PowerCSharp.Extensions;

var numbers = new List<int> { 1, 2, 3, 4, 5 };
bool isEmpty = numbers.IsNullOrEmpty(); // false
var first = numbers.FirstOrDefaultSafe(-1); // 1
var page = numbers.Page(1, 2); // [1, 2]
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

- [API Documentation](docs/) (Coming soon)
- [Examples and Samples](samples/)
- [Migration Guide](docs/migration.md) (Coming soon)

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
