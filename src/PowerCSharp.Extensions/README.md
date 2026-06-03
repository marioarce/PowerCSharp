# PowerCSharp.Extensions

![PowerCSharp Banner](../docs/images/PowerCSharp_Banner.png)

[![PowerCSharp.Extensions](https://img.shields.io/badge/PowerCSharp.Extensions-v0.3.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Extensions.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Extensions.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions)

Cross-platform extension methods for .NET developers that enhance productivity and simplify common programming tasks. This package contains over 100 extension methods organized into logical categories, compatible with both modern .NET and .NET Standard 2.0.

## 📦 Package Information

- **Package ID:** `PowerCSharp.Extensions`
- **Version:** 0.3.0
- **Target Frameworks:** .NET 8.0, .NET Standard 2.0
- **Dependencies:** 
  - `PowerCSharp.Core` (for shared interfaces)
  - `System.Linq.Dynamic.Core` v1.7.2 (for dynamic LINQ)
  - `Ben.Demystifier` v0.4.1 (for enhanced exception demystification)
  - `System.Text.Json` v10.0.8 (for JSON processing)

**Note:** ASP.NET Core specific extensions (Configuration, URI manipulation) are now available in the separate `PowerCSharp.Extensions.AspNetCore` package.

## 🚀 Installation

```bash
dotnet add package PowerCSharp.Extensions
```

## 📚 Extension Categories

### 🕒 DateTime Extensions

Enhanced date and time operations for common scenarios.

```csharp
using PowerCSharp.Extensions;

var date = DateTime.Now;
int age = date.GetAge();                    // Calculate age
bool isWeekend = date.IsWeekend();           // Check if weekend
var firstDay = date.FirstDayOfMonth();       // First day of month
var lastDay = date.LastDayOfMonth();         // Last day of month
```

### 🔤 String Extensions

Powerful string manipulation and validation methods.

```csharp
using PowerCSharp.Extensions;

string text = "hello world";
bool isEmpty = text.IsNullOrWhiteSpace();    // false
string title = text.ToTitleCase();            // "Hello World"
string camel = "HelloWorld".ToCamelCase();    // "helloWorld"
string safe = text.SafeSubstring(0, 5);       // "hello"
bool isValid = "https://example.com".IsValidUrl(); // true
```

### 📦 Collection Extensions

Enhanced collection operations for better data manipulation.

```csharp
using PowerCSharp.Extensions;

var numbers = new List<int> { 1, 2, 3, 4, 5 };
bool isEmpty = numbers.IsNullOrEmpty();       // false
var first = numbers.FirstOrDefaultSafe(-1);   // 1
var page = numbers.Page(1, 2);               // [1, 2]

// Remove all matching items
var list = new List<string> { "keep", "remove", "keep" };
int removed = list.RemoveAll(x => x == "remove"); // 1
```


### 🔍 LINQ & Dynamic Query Extensions

Advanced LINQ operations with dynamic expression parsing.

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

### 📄 JSON & XML Extensions

Simplified JSON and XML document manipulation.

```csharp
using PowerCSharp.Extensions;
using System.Text.Json;

// JSON element access
JsonElement element = JsonDocument.Parse("{\"name\":\"John\"}").RootElement;
var name = element.Get("name");               // JsonElement with value "John"
bool found = element.TryGetPropertyCaseInsensitive("NAME", out var value);

// XML flattening
using System.Xml.Linq;
XElement xml = XElement.Parse("<root><child>value</child></root>");
var dict = xml.Flatten();                     // Dictionary with XML structure
```

### 🏗️ Object & Type Extensions

Enhanced object manipulation and type operations.

```csharp
using PowerCSharp.Extensions;

// Object utilities
string text = "test";
text.ThrowOnNull();                           // Throws if null

bool isTrue = "true".TryGetBool(out bool result); // result = true

// Generic operations
var person = new Person { Name = "John", Age = 30 };
var copy = new Person();
person.CopyPropertiesTo(copy);                // Copies matching properties

// Type operations
bool isDefault = default(int).IsDefault();     // true
string typeName = typeof(List<string>).GetGenericTypeName(); // "List<String>"
```

### 🌊 Stream Extensions

Asynchronous stream operations and cloning.

```csharp
using PowerCSharp.Extensions;

using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes("test data"));
using var destinationStream = new MemoryStream();

await originalStream.CloneAsync(destinationStream);
// destinationStream now contains the same data as originalStream
```


## 🎯 Key Features

### ✅ Thread Safety
All extension methods are designed to be thread-safe when used with immutable data structures.

### ✅ Null Safety
Comprehensive null checking and graceful error handling throughout.

### ✅ Performance Optimized
Methods are optimized for performance with minimal allocations and efficient algorithms.

### ✅ .NET Standard Compatible
Works with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+, and .NET 8.0.

## 🔗 Dependencies

PowerCSharp.Extensions depends on:

- **PowerCSharp.Core** - Shared interfaces and base functionality
- **System.Linq.Dynamic.Core** - Dynamic LINQ expression parsing
- **Ben.Demystifier** - Enhanced exception demystification and stack trace formatting
- **System.Text.Json** - JSON processing

**For ASP.NET Core specific functionality:** Install `PowerCSharp.Extensions.AspNetCore` for configuration extensions, URI manipulation, and HTTP utilities.

## 🧪 Testing

PowerCSharp.Extensions includes comprehensive unit tests. Run tests with:

```bash
dotnet test src/PowerCSharp.Extensions.Tests
```

## 📖 Documentation

- [Main PowerCSharp Documentation](../../README.md) - Complete ecosystem overview
- [PowerCSharp.Core](../PowerCSharp.Core/README.md) - Core interfaces and architecture
- [PowerCSharp.Extensions.AspNetCore](../PowerCSharp.Extensions.AspNetCore/README.md) - ASP.NET Core specific extensions
- [Detailed API Documentation](../../docs/PowerCSharp.Extensions-API.md) - Complete API reference
- [Contributing Guide](../../CONTRIBUTING.md) - How to contribute
- [Workflow Documentation](../../docs/WORKFLOW.md) - Development workflow

## 💡 Usage Examples

### Dynamic LINQ Scenario
```csharp
using PowerCSharp.Extensions;

public class AdvancedDataProcessor
{
    public IEnumerable<User> FilterAndSortUsers(IEnumerable<User> users, string filterExpression, string sortExpression)
    {
        // Dynamic filtering
        if (!string.IsNullOrEmpty(filterExpression))
        {
            var predicate = filterExpression.GetExpressionDelegate<User>();
            users = users.Where(predicate);
        }
        
        // Dynamic sorting
        if (!string.IsNullOrEmpty(sortExpression))
        {
            var orderDelegates = sortExpression.GetOrderDelegates<User>();
            users = users.OrderByMultiple(orderDelegates);
        }
        
        return users;
    }
    
    public void ProcessDynamicQuery(string query)
    {
        // Example: "Age > 25 AND Name.Contains('John') ORDER BY Name DESC, Age ASC"
        var parts = query.Split("ORDER BY", StringSplitOptions.RemoveEmptyEntries);
        var filterPart = parts[0].Trim();
        var sortPart = parts.Length > 1 ? parts[1].Trim() : "Name ASC";
        
        var users = GetUsers();
        var results = FilterAndSortUsers(users, filterPart, sortPart);
        
        foreach (var user in results)
        {
            Console.WriteLine($"{user.Name} ({user.Age})");
        }
    }
    
    private IEnumerable<User> GetUsers() => new List<User>();
}

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](../../CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Navigate to PowerCSharp.Extensions project
3. Restore dependencies: `dotnet restore`
4. Run tests: `dotnet test`
5. Make your changes
6. Add tests for new functionality
7. Submit a Pull Request

### Adding New Extensions

When adding new extension methods:

1. **Choose the right category** - Place extensions in the appropriate folder
2. **Follow naming conventions** - Use descriptive, PascalCase method names
3. **Add XML documentation** - Include comprehensive XML docs
4. **Write unit tests** - Cover all scenarios and edge cases
5. **Update documentation** - Add to API documentation

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## 🔗 Related Packages

- [PowerCSharp.Core](../PowerCSharp.Core/README.md) - Core interfaces and architecture
- [PowerCSharp.Utilities](../PowerCSharp.Utilities/README.md) - Utility classes and helpers
- [PowerCSharp.Helpers](../PowerCSharp.Helpers/README.md) - Specialized helper classes

## 📞 Support

- 🐛 [Report Issues](https://github.com/marioarce/PowerCSharp/issues)
- 💡 [Feature Requests](https://github.com/marioarce/PowerCSharp/discussions)
- 📧 [Email Support](mailto:support@example.com)

---

**PowerCSharp.Extensions** - Making C# development more productive, one extension at a time! 🚀

[← Back to Main Documentation](../../README.md)
