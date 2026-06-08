# PowerCSharp.Core

![PowerCSharp Banner](../docs/images/PowerCSharp_Banner.png)

[![PowerCSharp.Core](https://img.shields.io/badge/PowerCSharp.Core-v0.3.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Core.svg)](https://www.nuget.org/packages/PowerCSharp.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Core.svg)](https://www.nuget.org/packages/PowerCSharp.Core)

The foundational core of the PowerCSharp ecosystem, providing centralized interfaces, models, and base functionality for all PowerCSharp packages. This package has been refactored to provide a clean, dependency-free foundation with improved architectural separation.

## 📦 Package Information

- **Package ID:** `PowerCSharp.Core`
- **Version:** 0.3.0
- **Target Frameworks:** .NET 8.0, .NET Standard 2.0
- **Dependencies:** None (dependency-free)

## 🏗️ Architecture Overview

PowerCSharp.Core serves as the architectural foundation for the entire PowerCSharp ecosystem:

### Centralized Interfaces
All interfaces are centralized in PowerCSharp.Core to maintain proper architectural separation:

- **PowerCSharp.Core.Interfaces.Extensions.Configuration** - Configuration-related interfaces
- **PowerCSharp.Core.Interfaces.Extensions.Linq** - LINQ and dynamic query interfaces
- **PowerCSharp.Core.Interfaces.Models** - Model classes (reserved for future use)

### Benefits
- **Single source of truth** for contracts and abstractions
- **Consistent namespace organization** across packages
- **Easy dependency management** across the ecosystem
- **Clear architectural boundaries** between packages

## 🚀 Installation

```bash
dotnet add package PowerCSharp.Core
```

## 📚 Available Interfaces

### Configuration Interfaces

#### IAppOptions

**Namespace:** `PowerCSharp.Core.Interfaces.Extensions.Configuration`

Represents the interface for application options.

```csharp
public interface IAppOptions
{
    string ConfigSectionPath { get; }
}
```

**Usage Example:**
```csharp
using PowerCSharp.Core.Interfaces.Extensions.Configuration;

public class MyAppOptions : IAppOptions
{
    public string ConfigSectionPath => "MyApp";
    public string ApiKey { get; set; }
    public int Timeout { get; set; }
}
```

### LINQ & Dynamic Query Interfaces

#### IDynamicFilterProvider<T>

**Namespace:** `PowerCSharp.Core.Interfaces.Extensions.Linq`

Implement a Service Provider for dynamically filtering of the type T using Dynamic Expressions.

```csharp
public interface IDynamicFilterProvider<T>
{
    void SetFilter(Func<T, bool> filter);
    Func<T, bool> GetFilter();
}
```

#### IDynamicOrderProvider<T>

**Namespace:** `PowerCSharp.Core.Interfaces.Extensions.Linq`

Implement a Service Provider for dynamically ordering of the type T using Dynamic Expressions.

```csharp
public interface IDynamicOrderProvider<T>
{
    void SetOrderDelegates(List<(Func<T, object>, bool)> delegates);
    List<(Func<T, object>, bool)> GetOrderDelegates();
}
```

**Usage Example:**
```csharp
using PowerCSharp.Core.Interfaces.Extensions.Linq;

// Create a dynamic filter provider
var filterProvider = new DynamicFilterProvider<Person>();
filterProvider.SetFilter(person => person.Age > 18 && person.Name.Contains("John"));

// Create a dynamic order provider
var orderProvider = new DynamicOrderProvider<Person>();
orderProvider.SetOrderDelegates(new List<(Func<Person, object>, bool)>
{
    (person => person.Name, false),  // Ascending by Name
    (person => person.Age, true)     // Descending by Age
});
```

## 🔗 Package Dependencies

PowerCSharp.Core has **no external dependencies**, making it lightweight and ideal for inclusion in any project.

Other PowerCSharp packages reference PowerCSharp.Core for:
- Shared interfaces and contracts
- Common base types
- Architectural consistency

## 🎯 Use Cases

### When to Use PowerCSharp.Core

- **Building custom PowerCSharp extensions** that need to implement standard interfaces
- **Creating applications** that use multiple PowerCSharp packages
- **Developing libraries** that need to integrate with PowerCSharp ecosystem
- **Ensuring type safety** across PowerCSharp packages

### Integration Examples

#### With PowerCSharp.Extensions
```csharp
using PowerCSharp.Extensions;
using PowerCSharp.Core.Interfaces.Extensions.Linq;

// Your custom filter implementation
public class PersonFilterProvider : IDynamicFilterProvider<Person>
{
    private Func<Person, bool> _filter;

    public void SetFilter(Func<Person, bool> filter) => _filter = filter;
    public Func<Person, bool> GetFilter() => _filter ?? (p => true);
}

// Use with PowerCSharp.Extensions
var people = new List<Person>();
var filtered = people.Filter(new PersonFilterProvider());
```

#### With Configuration
```csharp
using Microsoft.Extensions.Configuration;
using PowerCSharp.Extensions;
using PowerCSharp.Core.Interfaces.Extensions.Configuration;

public class DatabaseOptions : IAppOptions
{
    public string ConfigSectionPath => "Database";
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }
}

// Usage in application
var config = new ConfigurationBuilder().Build();
var dbOptions = config.GetOptions<DatabaseOptions>("Database");
```

## 🧪 Testing

PowerCSharp.Core includes comprehensive unit tests. Run tests with:

```bash
dotnet test src/PowerCSharp.Core.Tests
```

## 📖 Documentation

- [Main PowerCSharp Documentation](../../README.md) - Complete ecosystem overview
- [PowerCSharp.Extensions API Documentation](../../docs/PowerCSharp.Extensions-API.md) - Detailed API reference
- [Contributing Guide](../../CONTRIBUTING.md) - How to contribute
- [Workflow Documentation](../../docs/WORKFLOW.md) - Development workflow

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](../../CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Navigate to PowerCSharp.Core project
3. Run tests to ensure everything works
4. Make your changes
5. Add tests for new functionality
6. Submit a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## 🔗 Related Packages

- [PowerCSharp.Extensions](../PowerCSharp.Extensions/README.md) - Comprehensive extension methods
- [PowerCSharp.Utilities](../PowerCSharp.Utilities/README.md) - Utility classes and helpers
- [PowerCSharp.Helpers](../PowerCSharp.Helpers/README.md) - Specialized helper classes

## 📞 Support

- 🐛 [Report Issues](https://github.com/marioarce/PowerCSharp/issues)
- 💡 [Feature Requests](https://github.com/marioarce/PowerCSharp/discussions)
- 📧 [Email Support](mailto:support@example.com)

---

**PowerCSharp.Core** - The foundation of powerful C# development! 🚀

[← Back to Main Documentation](../../README.md)
