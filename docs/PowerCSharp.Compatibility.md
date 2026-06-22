# PowerCSharp.Compatibility API Reference

Complete API reference for PowerCSharp.Compatibility - .NET Framework compatibility layer with System.Web dependencies. This package provides a seamless migration path from .NET Framework to modern .NET while maintaining compatibility with existing PowerCSharp interfaces.

**Key Features:**
- **System.Web Integration**: URL manipulation and web utilities using System.Web
- **Async-to-Sync Bridging**: Safe async operations in synchronous contexts
- **Migration Path**: Gradual upgrade path from .NET Framework to modern .NET
- **Framework Compatibility**: Designed specifically for .NET Framework applications

> **Target Framework**: .NET Framework 4.6.2, 4.7.2, 4.8  
> **Namespace**: `PowerCSharp.Compatibility`

## 📋 Table of Contents

- [String Extensions](#-string-extensions)
- [Async Helper](#-async-helper)
- [Validation Utilities](#-validation-utilities)
- [Validation Attributes](#-validation-attributes)
- [Examples](#-examples)
- [Migration Guide](#-migration-guide)

## 🔤 String Extensions

### Namespace: `PowerCSharp.Compatibility.Extensions`

#### `AppendQueryParameters`

Appends query parameters to a URL using System.Web.HttpUtility.

```csharp
public static string AppendQueryParameters(this string url, IDictionary<string, string> parameters)
```

**Parameters:**
- `url` (string): The base URL to append parameters to
- `parameters` (IDictionary<string, string>): Dictionary of query parameters

**Returns:** string - The URL with appended query parameters

**Example:**
```csharp
using PowerCSharp.Compatibility.Extensions;

string baseUrl = "https://example.com/search";
var parameters = new Dictionary<string, string>
{
    ["q"] = "csharp tutorial",
    ["page"] = "1",
    ["lang"] = "en"
};

string result = baseUrl.AppendQueryParameters(parameters);
// Result: "https://example.com/search?q=csharp%20tutorial&page=1&lang=en"
```

**Behavior:**
- Handles existing query parameters correctly
- URL-encodes parameter values automatically
- Preserves URL fragments
- Returns original URL if parameters dictionary is null or empty

**Dependencies:**
- `System.Web.HttpUtility.ParseQueryString`
- `System.Web.HttpUtility.UrlDecode`

---

## 🔄 Async Helper

### Namespace: `PowerCSharp.Compatibility.Helpers`

#### `AsyncHelper`

Provides utilities for safely bridging asynchronous operations to synchronous contexts.

##### `RunSync<T>`

Runs an asynchronous function synchronously and returns the result without causing deadlocks.

```csharp
public static T RunSync<T>(Func<Task<T>> func)
```

**Type Parameters:**
- `T`: The return type of the asynchronous operation

**Parameters:**
- `func` (Func<Task<T>>): The asynchronous function to execute

**Returns:** T - The result of the asynchronous operation

**Example:**
```csharp
using PowerCSharp.Compatibility.Helpers;

// In MVC filters or other synchronous contexts
public class MyFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var result = AsyncHelper.RunSync(() => GetUserDataAsync());
        filterContext.ActionParameters["userData"] = result;
    }
    
    private async Task<UserData> GetUserDataAsync()
    {
        return await userService.GetUserAsync();
    }
}
```

**Implementation Details:**
- Uses custom `TaskFactory` with `TaskScheduler.Default`
- Prevents deadlocks common in sync-over-async scenarios
- Avoids `ConfigureAwait(false)` issues in .NET Framework
- Safe for use in ASP.NET synchronization contexts

**When to Use:**
- MVC/Web API action filters
- ASP.NET Web Forms event handlers
- Legacy frameworks requiring sync methods
- Unit testing async code in sync contexts

**When NOT to Use:**
- Modern ASP.NET Core applications (use async/await throughout)
- Performance-critical code (sync-over-async has overhead)
- New development (prefer async-first design)

---

## ✅ Validation Utilities

### Namespace: `PowerCSharp.Compatibility.Utilities`

#### `ValidationHelper`

Provides validation methods compatible with .NET Framework applications.

##### `IsValidEmail`

Validates email address format.

```csharp
public static bool IsValidEmail(string email)
```

**Parameters:**
- `email` (string): Email address to validate

**Returns:** bool - True if email format is valid

**Example:**
```csharp
using PowerCSharp.Compatibility.Utilities;

bool isValid = ValidationHelper.IsValidEmail("user@example.com"); // true
bool isInvalid = ValidationHelper.IsValidEmail("invalid-email"); // false
```

##### `IsNumeric`

Determines if a string represents a numeric value.

```csharp
public static bool IsNumeric(string value)
```

**Parameters:**
- `value` (string): String to test for numeric content

**Returns:** bool - True if the string can be parsed as a number

**Example:**
```csharp
using PowerCSharp.Compatibility.Utilities;

bool isNumber = ValidationHelper.IsNumeric("12345"); // true
bool notNumber = ValidationHelper.IsNumeric("abc123"); // false
bool decimalNumber = ValidationHelper.IsNumeric("123.45"); // true
```

##### `IsValidUrl`

Validates URL format.

```csharp
public static bool IsValidUrl(string url)
```

**Parameters:**
- `url` (string): URL string to validate

**Returns:** bool - True if URL format is valid

**Example:**
```csharp
using PowerCSharp.Compatibility.Utilities;

bool valid = ValidationHelper.IsValidUrl("https://example.com"); // true
bool invalid = ValidationHelper.IsValidUrl("not-a-url"); // false
```

---

## 🏷️ Validation Attributes

### Namespace: `PowerCSharp.Compatibility.Utilities.Attributes`

#### `ValidatedNotNullAttribute`

Attribute to indicate that a parameter should not be null, used for static analysis and validation.

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ValidatedNotNullAttribute : Attribute
{
}
```

**Usage:**
```csharp
using PowerCSharp.Compatibility.Utilities.Attributes;

public void ProcessData([ValidatedNotNull] string input)
{
    if (input == null)
    {
        throw new ArgumentNullException(nameof(input));
    }
    
    // Process data
}
```

**Purpose:**
- Static analysis support
- Code documentation
- Integration with validation frameworks
- Design-time null checking

---

## 🎯 Examples

### Complete Web Forms Example

```csharp
using PowerCSharp.Compatibility.Extensions;
using PowerCSharp.Compatibility.Helpers;
using PowerCSharp.Compatibility.Utilities;
using PowerCSharp.Compatibility.Utilities.Attributes;

public partial class SearchPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadSearchResults();
        }
    }
    
    private void LoadSearchResults()
    {
        // Build search URL with query parameters
        var searchParams = new Dictionary<string, string>
        {
            ["q"] = Request.QueryString["q"] ?? "",
            ["category"] = ddlCategories.SelectedValue,
            ["page"] = Request.QueryString["page"] ?? "1"
        };
        
        string searchUrl = "~/Search.aspx".AppendQueryParameters(searchParams);
        
        // Validate search query
        if (!string.IsNullOrEmpty(searchParams["q"]))
        {
            var results = AsyncHelper.RunSync(() => 
                SearchService.SearchAsync(searchParams["q"]));
            
            gvResults.DataSource = results;
            gvResults.DataBind();
        }
    }
    
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        // Validate input
        if (!ValidateSearchInput(txtSearch.Text))
        {
            lblError.Text = "Invalid search query";
            return;
        }
        
        // Redirect with search parameters
        var parameters = new Dictionary<string, string>
        {
            ["q"] = txtSearch.Text,
            ["category"] = ddlCategories.SelectedValue
        };
        
        string redirectUrl = Request.Url.AbsolutePath.AppendQueryParameters(parameters);
        Response.Redirect(redirectUrl);
    }
    
    private bool ValidateSearchInput([ValidatedNotNull] string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;
            
        // Additional validation logic
        return input.Length >= 2 && input.Length <= 100;
    }
}
```

### MVC 5 Filter Example

```csharp
using PowerCSharp.Compatibility.Helpers;
using PowerCSharp.Compatibility.Utilities;
using System.Web.Mvc;

public class ApiAuthorizationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var apiKey = filterContext.HttpContext.Request.Headers["X-API-Key"];
        
        // Validate API key format
        if (string.IsNullOrEmpty(apiKey) || !ValidationHelper.IsNumeric(apiKey))
        {
            filterContext.Result = new HttpStatusCodeResult(401, "Invalid API Key");
            return;
        }
        
        // Async authorization check
        var isAuthorized = AsyncHelper.RunSync(() => 
            AuthorizationService.ValidateApiKeyAsync(apiKey));
        
        if (!isAuthorized)
        {
            filterContext.Result = new HttpStatusCodeResult(403, "Access Denied");
            return;
        }
        
        base.OnActionExecuting(filterContext);
    }
}
```

### Legacy API Service Example

```csharp
using PowerCSharp.Compatibility.Extensions;
using PowerCSharp.Compatibility.Utilities;

public class LegacyApiService
{
    private readonly string baseUrl;
    
    public LegacyApiService(string baseUrl)
    {
        if (!ValidationHelper.IsValidUrl(baseUrl))
            throw new ArgumentException("Invalid base URL", nameof(baseUrl));
            
        this.baseUrl = baseUrl;
    }
    
    public string BuildApiUrl(string endpoint, object parameters)
    {
        var fullUrl = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        
        if (parameters != null)
        {
            var paramDict = ConvertToDictionary(parameters);
            return fullUrl.AppendQueryParameters(paramDict);
        }
        
        return fullUrl;
    }
    
    public bool ValidateApiRequest(string requestJson)
    {
        // Basic validation for API requests
        if (string.IsNullOrEmpty(requestJson))
            return false;
            
        // Could add more complex validation logic here
        return requestJson.Length > 0 && requestJson.Length < 10000;
    }
    
    private Dictionary<string, string> ConvertToDictionary(object obj)
    {
        var dict = new Dictionary<string, string>();
        
        foreach (var prop in obj.GetType().GetProperties())
        {
            var value = prop.GetValue(obj);
            if (value != null)
            {
                dict[prop.Name] = value.ToString();
            }
        }
        
        return dict;
    }
}
```

---

## 🔄 Migration Guide

### From PowerCSharp.Compatibility to Modern PowerCSharp

#### String Extensions Migration

**PowerCSharp.Compatibility (.NET Framework):**
```csharp
using PowerCSharp.Compatibility.Extensions;

string url = "https://example.com".AppendQueryParameters(params);
```

**PowerCSharp.Extensions (Modern .NET):**
```csharp
using PowerCSharp.Extensions;

Uri uri = new Uri("https://example.com").AddParameter("key", "value");
```

#### Async Helper Migration

**PowerCSharp.Compatibility (.NET Framework):**
```csharp
using PowerCSharp.Compatibility.Helpers;

var result = AsyncHelper.RunSync(() => GetDataAsync());
```

**Modern .NET (Preferred):**
```csharp
// In modern .NET, use async/await throughout
var result = await GetDataAsync();
```

#### Validation Migration

**PowerCSharp.Compatibility (.NET Framework):**
```csharp
using PowerCSharp.Compatibility.Utilities;

bool isValid = ValidationHelper.IsValidEmail(email);
```

**PowerCSharp.Utilities (Modern .NET):**
```csharp
using PowerCSharp.Utilities;

bool isValid = ValidationHelper.IsValidEmail(email);
```

### Step-by-Step Migration Process

1. **Phase 1: Add Modern Packages**
   ```bash
   dotnet add package PowerCSharp.Extensions
   dotnet add package PowerCSharp.Utilities
   ```

2. **Phase 2: Replace Usages**
   - Update using statements
   - Replace method calls with modern equivalents
   - Remove AsyncHelper usage (convert to async/await)

3. **Phase 3: Remove Compatibility Package**
   ```bash
   dotnet remove package PowerCSharp.Compatibility
   ```

4. **Phase 4: Update Target Framework**
   ```xml
   <TargetFramework>net8.0</TargetFramework>
   ```

---

## 📝 Additional Notes

### Performance Considerations

- **AsyncHelper.RunSync**: Has performance overhead due to thread blocking
- **StringExtensions.AppendQueryParameters**: Uses System.Web.HttpUtility which may be slower than modern alternatives
- **ValidationHelper**: Uses regex patterns optimized for .NET Framework

### Security Considerations

- **URL Parameter Handling**: All parameters are URL-encoded automatically
- **Input Validation**: Validation methods use safe parsing techniques
- **Async Bridging**: AsyncHelper prevents deadlock vulnerabilities

### Compatibility Notes

- **.NET Framework 4.6.2**: Minimum supported version
- **System.Web Dependency**: Requires IIS hosting environment
- **Windows Only**: System.Web is Windows-specific
- **No Cross-Platform**: Designed for traditional Windows Server deployments

---

## 🔗 Related Documentation

- [PowerCSharp.Compatibility README](../src/PowerCSharp.Compatibility/README.md) - Package overview and getting started
- [Main PowerCSharp Documentation](../README.md) - Complete ecosystem documentation
- [PowerCSharp.Extensions API](PowerCSharp.Extensions.md) - Modern extension methods
- [PowerCSharp.Utilities API](PowerCSharp.Utilities.md) - Modern utilities

---

**PowerCSharp.Compatibility** - API Reference for .NET Framework development! 🚀
