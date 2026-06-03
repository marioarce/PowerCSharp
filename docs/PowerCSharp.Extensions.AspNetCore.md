# PowerCSharp.Extensions.AspNetCore API Documentation

## Overview

PowerCSharp.Extensions.AspNetCore provides ASP.NET Core specific extension methods that require ASP.NET Core dependencies. This package focuses on configuration management, web utilities, and HTTP operations that are specifically designed for modern ASP.NET Core applications.

## Target Framework

- **.NET 8.0** - Leverages the latest ASP.NET Core features and optimizations

## Dependencies

- **PowerCSharp.Core** v0.3.0 - Shared interfaces and base functionality
- **Microsoft.AspNetCore.WebUtilities** v8.0.8 - URL query string manipulation and web utilities
- **Microsoft.Extensions.Configuration.Abstractions** v8.0.0 - Configuration support
- **Microsoft.Extensions.Configuration.Binder** v8.0.0 - Configuration binding
- **Microsoft.Extensions.DependencyInjection** v8.0.0 - Dependency injection support
- **Microsoft.Extensions.Options** v8.0.0 - Options pattern support

## Recent Updates (v0.3.0)

- **Package Compatibility**: Resolved NuGet package compatibility issues for .NET 8.0
- **Dependency Updates**: Updated all Microsoft.Extensions packages to v8.0.x for full .NET 8.0 compatibility
- **Build Improvements**: Enhanced package generation with symbol packages

## Namespaces

### PowerCSharp.Extensions.AspNetCore.Configuration

Contains extension methods for configuration operations in ASP.NET Core applications.

### PowerCSharp.Extensions.AspNetCore.Net

Contains extension methods for network operations, HTTP utilities, and URI manipulation.

## API Reference

### Configuration Extensions

#### ConfigurationExtensions

Located in: `PowerCSharp.Extensions.AspNetCore.Configuration`

##### GetOptions<TOptions>(this IConfiguration configuration, string sectionPath)

Gets options from IConfiguration object using the specified configuration section path.

**Parameters:**
- `configuration` (IConfiguration): The IConfiguration object to read from
- `sectionPath` (string): The configuration section path to use

**Returns:**
- `TOptions`: The configured options object

**Type Parameters:**
- `TOptions`: The type of options to retrieve. Must implement IAppOptions

**Exceptions:**
- `ArgumentNullException`: Thrown when configuration is null
- `ArgumentException`: Thrown when sectionPath is null or whitespace
- `InvalidOperationException`: Thrown when configuration binding fails

**Example:**
```csharp
using PowerCSharp.Extensions.AspNetCore;
using Microsoft.Extensions.Configuration;

public class AppSettings : IAppOptions
{
    public string ApiUrl { get; set; } = "";
    public int MaxRetries { get; set; } = 3;
}

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var settings = configuration.GetOptions<AppSettings>("AppSettings");
```

### Network Extensions

#### UriExtensions

Located in: `PowerCSharp.Extensions.AspNetCore.Net`

##### AddParameter(this Uri url, string parameterName, string parameterValue)

Adds the specified parameter to the query string of the URI.

**Parameters:**
- `url` (Uri): The base URI to add the parameter to
- `parameterName` (string): The name of the parameter to add
- `parameterValue` (string): The value for the parameter to add

**Returns:**
- `Uri`: A new URI with the added parameter in the query string

**Example:**
```csharp
using PowerCSharp.Extensions.AspNetCore;

var baseUrl = new Uri("https://api.example.com/users");
var urlWithPage = baseUrl.AddParameter("page", "2");
var urlWithFilters = urlWithPage.AddParameter("search", "john");
// Result: https://api.example.com/users?page=2&search=john
```

**Notes:**
- Automatically handles URL encoding of parameter values
- Preserves existing query string parameters
- Creates a new Uri instance (immutable operation)

#### HttpRequestMessageExtensions

Located in: `PowerCSharp.Extensions.AspNetCore.Net`

##### Clone(this HttpRequestMessage original)

Creates a deep clone of the HttpRequestMessage including headers, content, and properties.

**Parameters:**
- `original` (HttpRequestMessage): The original HttpRequestMessage to clone

**Returns:**
- `HttpRequestMessage`: A new HttpRequestMessage instance with all headers, content, and properties copied

**Exceptions:**
- `ArgumentNullException`: Thrown when the original request is null

**Example:**
```csharp
using PowerCSharp.Extensions.AspNetCore;
using System.Net.Http;

var originalRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
originalRequest.Headers.Add("Authorization", "Bearer token123");

var clonedRequest = originalRequest.Clone();
// clonedRequest has the same method, URI, headers, and content as original
```

##### CloneAsync(this HttpRequestMessage original, CancellationToken cancellationToken = default)

Creates a deep clone of the HttpRequestMessage asynchronously for better performance with large content.

**Parameters:**
- `original` (HttpRequestMessage): The original HttpRequestMessage to clone
- `cancellationToken` (CancellationToken): A cancellation token to cancel the operation

**Returns:**
- `Task<HttpRequestMessage>`: A task that represents the asynchronous operation, containing the cloned HttpRequestMessage

**Exceptions:**
- `ArgumentNullException`: Thrown when the original request is null

**Example:**
```csharp
using PowerCSharp.Extensions.AspNetCore;
using System.Net.Http;

var originalRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/upload");
originalRequest.Content = new StringContent(largeJsonData, Encoding.UTF8, "application/json");

var clonedRequest = await originalRequest.CloneAsync();
// More efficient for large content as it reads asynchronously
```

**Notes:**
- Essential for retry scenarios where HttpRequestMessage instances need to be recreated
- HttpRequestMessage instances can only be sent once per .NET guidelines
- Async version is recommended for requests with large content

#### HttpStatusCodeExtensions

Located in: `PowerCSharp.Extensions.AspNetCore.Net`

##### IsSuccessful(this HttpStatusCode statusCode)

Determines if the HTTP status code indicates a successful request (2xx range).

**Parameters:**
- `statusCode` (HttpStatusCode): The HTTP status code to check

**Returns:**
- `bool`: True if the status code is in the 2xx range, otherwise false

**Example:**
```csharp
using PowerCSharp.Extensions.AspNetCore;
using System.Net;

var response = await httpClient.GetAsync("https://api.example.com/data");
bool isSuccess = response.StatusCode.IsSuccessful(); // true for 200 OK, false for 404 Not Found
```

##### IsClientError(this HttpStatusCode statusCode)

Determines if the HTTP status code indicates a client error (4xx range).

**Parameters:**
- `statusCode` (HttpStatusCode): The HTTP status code to check

**Returns:**
- `bool`: True if the status code is in the 4xx range, otherwise false

**Example:**
```csharp
bool isClientError = HttpStatusCode.BadRequest.IsClientError(); // true
bool isServerError = HttpStatusCode.InternalServerError.IsClientError(); // false
```

##### IsServerError(this HttpStatusCode statusCode)

Determines if the HTTP status code indicates a server error (5xx range).

**Parameters:**
- `statusCode` (HttpStatusCode): The HTTP status code to check

**Returns:**
- `bool`: True if the status code is in the 5xx range, otherwise false

**Example:**
```csharp
bool isServerError = HttpStatusCode.InternalServerError.IsServerError(); // true
bool isClientError = HttpStatusCode.NotFound.IsServerError(); // false
```

##### IsRedirect(this HttpStatusCode statusCode)

Determines if the HTTP status code indicates a redirect (3xx range).

**Parameters:**
- `statusCode` (HttpStatusCode): The HTTP status code to check

**Returns:**
- `bool`: True if the status code is in the 3xx range, otherwise false

**Example:**
```csharp
bool isRedirect = HttpStatusCode.Found.IsRedirect(); // true
bool isSuccess = HttpStatusCode.OK.IsRedirect(); // false
```

## Usage Patterns

### Configuration Management

```csharp
public class DatabaseSettings : IAppOptions
{
    public string ConnectionString { get; set; } = "";
    public int CommandTimeout { get; set; } = 30;
    public bool EnableRetry { get; set; } = true;
}

// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    var configuration = Configuration;
    var dbSettings = configuration.GetOptions<DatabaseSettings>("DatabaseSettings");
    
    services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
    
    // Use settings
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(dbSettings.ConnectionString));
}
```

### HTTP Client with Retry Logic

```csharp
public class ResilientApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResilientApiClient> _logger;

    public async Task<T> GetWithRetryAsync<T>(string endpoint, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                
                if (!response.StatusCode.IsServerError() && !response.StatusCode.IsClientError())
                {
                    break; // Don't retry on success or redirect codes
                }
                
                _logger.LogWarning("Request failed with status {StatusCode}, attempt {Attempt}/{MaxRetries}", 
                    response.StatusCode, attempt, maxRetries);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                }
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogError(ex, "Request failed on attempt {Attempt}", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }
        
        throw new HttpRequestException($"Failed to get {endpoint} after {maxRetries} attempts");
    }
}
```

### URL Building with Query Parameters

```csharp
public class UrlBuilder
{
    public Uri BuildSearchUrl(string baseUrl, SearchParameters parameters)
    {
        var uri = new Uri(baseUrl);
        
        if (!string.IsNullOrEmpty(parameters.Query))
        {
            uri = uri.AddParameter("q", parameters.Query);
        }
        
        if (parameters.Page > 1)
        {
            uri = uri.AddParameter("page", parameters.Page.ToString());
        }
        
        if (parameters.PageSize != 20)
        {
            uri = uri.AddParameter("size", parameters.PageSize.ToString());
        }
        
        if (parameters.SortBy != "relevance")
        {
            uri = uri.AddParameter("sort", parameters.SortBy);
        }
        
        if (parameters.SortDescending)
        {
            uri = uri.AddParameter("order", "desc");
        }
        
        return uri;
    }
}
```

## Best Practices

### Configuration

1. **Always validate options** after loading them from configuration
2. **Use strongly-typed options classes** that implement IAppOptions
3. **Provide sensible defaults** for optional configuration values
4. **Use environment-specific configuration** files for different deployment scenarios

### HTTP Operations

1. **Use Clone() for retry scenarios** - HttpRequestMessage can only be sent once
2. **Prefer CloneAsync() for large payloads** to avoid blocking operations
3. **Check status code categories** using the provided extension methods
4. **Implement proper error handling** with exponential backoff for retries

### URL Manipulation

1. **Chain AddParameter() calls** for multiple query parameters
2. **Let the extension handle encoding** - don't pre-encode parameter values
3. **Use Uri objects** instead of string concatenation for URL building

## Performance Considerations

- **ConfigurationExtensions.GetOptions()** uses efficient reflection-based binding
- **UriExtensions.AddParameter()** creates new Uri instances (immutable)
- **HttpRequestMessageExtensions.Clone()** performs deep copy of all properties
- **HttpRequestMessageExtensions.CloneAsync()** is more efficient for large content

## Thread Safety

All extension methods in PowerCSharp.Extensions.AspNetCore are thread-safe when used with immutable data structures. Configuration operations should be performed during application startup, and HTTP operations should use proper synchronization when shared across threads.

## Error Handling

The extensions provide comprehensive error handling:

- **ArgumentNullException** for null parameters
- **ArgumentException** for invalid parameter values
- **InvalidOperationException** for configuration binding failures
- **Meaningful error messages** to aid debugging

## See Also

- [PowerCSharp.Core API Documentation](PowerCSharp.Core.md) - Core interfaces and models
- [PowerCSharp.Extensions API Documentation](PowerCSharp.Extensions.md) - Cross-platform extensions
- [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration) - Official configuration documentation
- [Microsoft.AspNetCore.WebUtilities](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests) - Official web utilities documentation
