# PowerCSharp.Extensions

![PowerCSharp Banner](../docs/images/PowerCSharp_Banner.png)

[![PowerCSharp.Extensions](https://img.shields.io/badge/PowerCSharp.Extensions-v0.1.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Extensions.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Extensions.svg)](https://www.nuget.org/packages/PowerCSharp.Extensions)

Comprehensive extension methods for .NET developers that enhance productivity and simplify common programming tasks. This package contains over 100 extension methods organized into logical categories.

## 📦 Package Information

- **Package ID:** `PowerCSharp.Extensions`
- **Version:** 0.1.0
- **Target Frameworks:** .NET 8.0, .NET Standard 2.0
- **Dependencies:** 
  - `PowerCSharp.Core` (for shared interfaces)
  - `System.Linq.Dynamic.Core` (for dynamic LINQ)
  - `Microsoft.AspNetCore.WebUtilities`
  - `Microsoft.Extensions.Configuration.Abstractions`
  - `Microsoft.Extensions.Configuration.Binder`
  - `System.Text.Json`

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

### 🌐 HTTP & Network Extensions

Simplified HTTP status code handling and URL manipulation.

```csharp
using PowerCSharp.Extensions;
using System.Net;

HttpStatusCode status = HttpStatusCode.OK;
bool success = status.IsSuccessful();         // true
bool clientError = status.IsClientError();    // false
bool serverError = status.IsServerError();     // false

// URL manipulation
Uri uri = new Uri("https://example.com");
Uri withParam = uri.AddParameter("search", "test"); // https://example.com?search=test

// HTTP Request cloning
using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com");
var clonedRequest = request.Clone();
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

### ⚙️ Configuration Extensions

Simplified configuration binding and options management.

```csharp
using PowerCSharp.Extensions;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder().Build();
var options = configuration.GetOptions<MyAppOptions>("MyApp"); // Reads from "MyApp" section
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
- **Microsoft.AspNetCore.WebUtilities** - URL query string manipulation
- **Microsoft.Extensions.Configuration.Abstractions** - Configuration support
- **Microsoft.Extensions.Configuration.Binder** - Configuration binding
- **System.Text.Json** - JSON processing

## 🧪 Testing

PowerCSharp.Extensions includes comprehensive unit tests. Run tests with:

```bash
dotnet test src/PowerCSharp.Extensions.Tests
```

## 📖 Documentation

- [Main PowerCSharp Documentation](../../README.md) - Complete ecosystem overview
- [PowerCSharp.Core](../PowerCSharp.Core/README.md) - Core interfaces and architecture
- [Detailed API Documentation](../../docs/PowerCSharp.Extensions-API.md) - Complete API reference
- [Contributing Guide](../../CONTRIBUTING.md) - How to contribute
- [Workflow Documentation](../../docs/WORKFLOW.md) - Development workflow

## 💡 Usage Examples

### Web API Scenario
```csharp
using PowerCSharp.Extensions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsers([FromQuery] string filter = "", [FromQuery] string sort = "Name")
    {
        var users = _userRepository.GetAll();
        
        // Apply dynamic filtering
        if (!string.IsNullOrEmpty(filter))
        {
            var filterProvider = new DynamicFilterProvider<User>();
            filterProvider.SetFilter(u => u.Name.Contains(filter) || u.Email.Contains(filter));
            users = users.Filter(filterProvider);
        }
        
        // Apply dynamic sorting
        var orderProvider = new DynamicOrderProvider<User>();
        var orderDelegates = sort.GetOrderDelegates<User>();
        orderProvider.SetOrderDelegates(orderDelegates);
        users = users.Order(orderProvider);
        
        return Ok(users);
    }
}
```

### Data Processing Scenario
```csharp
using PowerCSharp.Extensions;

public class DataProcessor
{
    public void ProcessRecords(List<DataRecord> records)
    {
        // Validate input
        records.ThrowOnNull();
        
        if (records.IsNullOrEmpty())
        {
            Console.WriteLine("No records to process");
            return;
        }
        
        // Process in pages
        int pageSize = 100;
        int pageNumber = 1;
        
        while (true)
        {
            var page = records.Page(pageNumber, pageSize);
            if (!page.Any()) break;
            
            foreach (var record in page)
            {
                // Process individual record
                ProcessRecord(record);
            }
            
            pageNumber++;
        }
    }
    
    private void ProcessRecord(DataRecord record)
    {
        // String operations
        record.Name = record.Name?.ToTitleCase() ?? "";
        record.Email = record.Email?.ToLowerInvariant() ?? "";
        
        // Validation
        if (!record.Email.IsValidEmail())
        {
            throw new InvalidOperationException($"Invalid email: {record.Email}");
        }
        
        // Date operations
        if (record.CreatedDate.IsWeekend())
        {
            record.Priority = "Low";
        }
        
        record.Age = DateTime.Now.GetAge(record.BirthDate);
    }
}
```

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
