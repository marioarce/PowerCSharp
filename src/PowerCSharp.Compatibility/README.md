# PowerCSharp.Compatibility

![PowerCSharp Banner](../../docs/images/PowerCSharp_Banner.png)

[![PowerCSharp.Compatibility](https://img.shields.io/badge/PowerCSharp.Compatibility-v0.1.0-blue.svg)](https://github.com/marioarce/PowerCSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/PowerCSharp.Compatibility.svg)](https://www.nuget.org/packages/PowerCSharp.Compatibility)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PowerCSharp.Compatibility.svg)](https://www.nuget.org/packages/PowerCSharp.Compatibility)

**.NET Framework compatibility layer for PowerCSharp** - Essential utilities and extensions for legacy .NET Framework applications with System.Web dependencies.

> **⚠️ .NET Framework Exclusive**: This package is specifically designed for .NET Framework applications (4.6.2, 4.7.2, 4.8) and includes System.Web dependencies. For modern .NET applications, use other PowerCSharp packages.

## 📦 Package Information

- **Package ID:** `PowerCSharp.Compatibility`
- **Version:** 0.1.0
- **Target Frameworks:** .NET Framework 4.6.2, 4.7.2, 4.8
- **Dependencies:** 
  - `System.Text.Json` v10.0.8 (modern JSON handling)
  - `System.Web` (Framework reference)

## 🎯 Why PowerCSharp.Compatibility?

### .NET Framework Legacy Support
PowerCSharp.Compatibility bridges the gap between modern PowerCSharp utilities and legacy .NET Framework applications, providing:

- **System.Web Integration** - URL manipulation and web utilities using System.Web
- **Async-to-Sync Bridging** - Safe async operations in synchronous contexts
- **Framework-Specific Validation** - Validation helpers compatible with older frameworks
- **Migration Path** - Gradual upgrade path from .NET Framework to modern .NET

### Key Scenarios
- **ASP.NET Web Forms** applications requiring modern utilities
- **MVC 5** projects needing async compatibility
- **Web API** applications with System.Web dependencies
- **Legacy enterprise** applications gradual modernization

## 🚀 Installation

```bash
dotnet add package PowerCSharp.Compatibility
```

**Package Manager Console:**
```powershell
Install-Package PowerCSharp.Compatibility
```

## 📚 Available Components

### 🔤 String Extensions (System.Web)

Enhanced string manipulation with System.Web.HttpUtility integration.

```csharp
using PowerCSharp.Compatibility.Extensions;

// URL query parameter manipulation
string baseUrl = "https://example.com/search";
var parameters = new Dictionary<string, string>
{
    ["q"] = "csharp tutorial",
    ["page"] = "1",
    ["lang"] = "en"
};

string fullUrl = baseUrl.AppendQueryParameters(parameters);
// Result: "https://example.com/search?q=csharp%20tutorial&page=1&lang=en"
```

### 🔄 Async Helper

Safely bridge async operations to synchronous contexts without deadlocks.

```csharp
using PowerCSharp.Compatibility.Helpers;

// In MVC filters or other synchronous contexts
public class MyMvcFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        // Safely call async methods from sync context
        var result = AsyncHelper.RunSync(() => GetUserDataAsync(filterContext.HttpContext.User));
        
        filterContext.ActionParameters["userData"] = result;
        base.OnActionExecuting(filterContext);
    }
    
    private async Task<UserData> GetUserDataAsync(IPrincipal user)
    {
        // Your async operation here
        return await userService.GetUserAsync(user.Identity.Name);
    }
}
```

### ✅ Validation Utilities

Framework-compatible validation helpers with custom attributes.

```csharp
using PowerCSharp.Compatibility.Utilities;
using PowerCSharp.Compatibility.Utilities.Attributes;

// Method parameter validation
public void ProcessData([ValidatedNotNull] string input, int count)
{
    if (string.IsNullOrEmpty(input))
        throw new ArgumentException("Input cannot be null or empty");
    
    // Additional validation logic
}

// Validation helper usage
bool isValid = ValidationHelper.IsValidEmail("user@example.com");
bool isNumeric = ValidationHelper.IsNumeric("12345");
```

## 🏗️ Architecture

### Package Structure
```
PowerCSharp.Compatibility/
├── Extensions/
│   └── StringExtensions.cs          # System.Web URL utilities
├── Helpers/
│   └── AsyncHelper.cs               # Async-to-sync bridging
└── Utilities/
    ├── Attributes/
    │   └── ValidatedNotNullAttribute.cs  # Validation attributes
    └── ValidationHelper.cs          # Framework-compatible validation
```

### Dependencies
- **System.Web** - Core .NET Framework web utilities
- **System.Text.Json** - Modern JSON handling (backported)
- **No external dependencies** - Pure .NET Framework compatibility

## 🔄 Migration Scenarios

### From .NET Framework to Modern .NET

PowerCSharp.Compatibility provides a smooth migration path:

1. **Phase 1**: Add PowerCSharp.Compatibility to existing .NET Framework project
2. **Phase 2**: Replace custom utilities with PowerCSharp equivalents
3. **Phase 3**: Migrate to modern .NET and switch to other PowerCSharp packages

```csharp
// Phase 1: .NET Framework with PowerCSharp.Compatibility
using PowerCSharp.Compatibility.Extensions;
string url = oldUrl.AppendQueryParameters(params);

// Phase 3: Modern .NET with PowerCSharp.Extensions  
using PowerCSharp.Extensions;
Uri uri = new Uri(oldUrl).AddParameter("key", "value");
```

### Integration with Existing PowerCSharp Packages

```csharp
// .NET Framework application
using PowerCSharp.Compatibility.Extensions;      // For System.Web utilities
using PowerCSharp.Compatibility.Helpers;         // For async bridging
using PowerCSharp.Core;                          // Shared interfaces
```

## 🎯 Target Framework Compatibility

| Framework | Version | Support | Notes |
|-----------|---------|---------|-------|
| .NET Framework | 4.6.2+ | ✅ Full | Minimum supported version |
| .NET Framework | 4.7.2 | ✅ Full | Recommended version |
| .NET Framework | 4.8 | ✅ Full | Latest Framework version |
| .NET Core | Any | ❌ No | Use PowerCSharp.Extensions instead |
| .NET 5+ | Any | ❌ No | Use PowerCSharp.Extensions instead |
| .NET Standard | Any | ❌ No | Use PowerCSharp.Extensions instead |

## 🧪 Testing

PowerCSharp.Compatibility includes comprehensive unit tests for all target frameworks:

```bash
# Test all frameworks
dotnet test src/PowerCSharp.Compatibility.Tests

# Test specific framework
dotnet test src/PowerCSharp.Compatibility.Tests --framework net48
```

## 📖 Usage Examples

### ASP.NET Web Forms Integration

```csharp
using PowerCSharp.Compatibility.Extensions;
using PowerCSharp.Compatibility.Helpers;

public partial class SearchPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Build search URL with parameters
            var searchParams = new Dictionary<string, string>
            {
                ["q"] = Request.QueryString["q"] ?? "",
                ["category"] = ddlCategories.SelectedValue,
                ["page"] = "1"
            };
            
            string searchUrl = "~/Search.aspx".AppendQueryParameters(searchParams);
            Response.Redirect(searchUrl);
        }
    }
    
    protected async void btnSearch_Click(object sender, EventArgs e)
    {
        // In Web Forms, you might need to call async from sync
        var results = AsyncHelper.RunSync(() => SearchService.SearchAsync(txtSearch.Text));
        gvResults.DataSource = results;
        gvResults.DataBind();
    }
}
```

### MVC 5 Filter with Async Operations

```csharp
using PowerCSharp.Compatibility.Helpers;
using System.Web.Mvc;

public class UserAuthorizationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var user = filterContext.HttpContext.User;
        
        // Safely call async authorization service
        var isAuthorized = AsyncHelper.RunSync(() => 
            AuthorizationService.IsUserAuthorizedAsync(user.Identity.Name));
        
        if (!isAuthorized)
        {
            filterContext.Result = new HttpNotFoundResult();
            return;
        }
        
        base.OnActionExecuting(filterContext);
    }
}
```

### Legacy API Integration

```csharp
using PowerCSharp.Compatibility.Extensions;
using PowerCSharp.Compatibility.Utilities;

public class LegacyApiService
{
    public string BuildApiUrl(string baseUrl, object parameters)
    {
        var dict = new Dictionary<string, string>();
        
        // Convert object to dictionary
        foreach (var prop in parameters.GetType().GetProperties())
        {
            var value = prop.GetValue(parameters)?.ToString() ?? "";
            if (!string.IsNullOrEmpty(value))
            {
                dict[prop.Name] = value;
            }
        }
        
        return baseUrl.AppendQueryParameters(dict);
    }
    
    public bool ValidateRequest(string request)
    {
        return !string.IsNullOrEmpty(request) && 
               ValidationHelper.IsNumeric(request.Substring(0, 4));
    }
}
```

## ⚠️ Important Considerations

### Async-to-Sync Usage
- **Use sparingly** - Only when absolutely necessary (MVC filters, Web Forms)
- **Avoid deadlocks** - AsyncHelper prevents common deadlock scenarios
- **Performance impact** - Sync-over-async has performance overhead

### System.Web Dependencies
- **Framework exclusive** - Requires System.Web assembly
- **IIS hosting** - Designed for web applications hosted in IIS
- **No cross-platform** - Windows-only due to System.Web

### Migration Planning
- **Gradual migration** - Use as stepping stone to modern .NET
- **Code compatibility** - APIs designed to match modern PowerCSharp packages
- **Testing strategy** - Comprehensive test coverage for migration validation

## 🔗 Related Packages

### Modern .NET Alternatives
- **[PowerCSharp.Extensions](../PowerCSharp.Extensions/README.md)** - Modern extension methods (.NET 8, .NET Standard)
- **[PowerCSharp.Helpers](../PowerCSharp.Helpers/README.md)** - Modern helper classes
- **[PowerCSharp.Utilities](../PowerCSharp.Utilities/README.md)** - Modern utility classes

### Foundation
- **[PowerCSharp.Core](../PowerCSharp.Core/README.md)** - Shared interfaces and contracts

## 📚 Documentation

- [Main PowerCSharp Documentation](../../README.md) - Complete ecosystem overview
- [PowerCSharp.Compatibility API Reference](../../docs/PowerCSharp.Compatibility.md) - Detailed API documentation
- [Migration Guide](../../docs/MIGRATION.md) - .NET Framework to modern .NET migration
- [Contributing Guide](../../CONTRIBUTING.md) - How to contribute

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](../../CONTRIBUTING.md) for details.

### Development Setup for .NET Framework

1. **Prerequisites**: Visual Studio 2019+ with .NET Framework development workload
2. **Clone repository**: `git clone https://github.com/marioarce/PowerCSharp.git`
3. **Navigate to project**: `cd src/PowerCSharp.Compatibility`
4. **Build**: `dotnet build` or open in Visual Studio
5. **Test**: `dotnet test --framework net48`

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## 📞 Support

- 🐛 [Report Issues](https://github.com/marioarce/PowerCSharp/issues)
- 💡 [Feature Requests](https://github.com/marioarce/PowerCSharp/discussions)
- 📧 [Email Support](mailto:support@example.com)

---

**PowerCSharp.Compatibility** - Keeping .NET Framework applications powerful and modern! 🚀

[← Back to Main Documentation](../../README.md)
