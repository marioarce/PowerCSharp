# PowerCSharp.Extensions - Detailed Documentation

## Overview

PowerCSharp.Extensions provides cross-platform extension methods for .NET developers that enhance productivity and simplify common programming tasks. This package contains over 100 extension methods organized into logical categories including strings, collections, LINQ, JSON, XML, objects, types, streams, and exception handling.

**Note:** ASP.NET Core specific extensions (Configuration, HTTP utilities, URI manipulation) are now available in the separate `PowerCSharp.Extensions.AspNetCore` package.

## Architecture

### Design Principles

1. **Fluent API**: Chainable methods for expressive code
2. **Null Safety**: Comprehensive null checking and handling
3. **Performance**: Optimized implementations for common scenarios
4. **Consistency**: Uniform naming and behavior patterns
5. **Extensibility**: Easy to extend with custom methods

### Namespace Structure

```
PowerCSharp.Extensions/
├── Collections/
│   ├── CollectionExtensions.cs
│   └── IListExtensions.cs
├── DateTimeExtensions.cs
├── EnumerableExtensions.cs
├── Json/
│   ├── JsonExtensions.cs
│   └── JsonElementExtensions.cs
├── Linq/
│   ├── DynamicExpressionExtensions.cs
│   └── IEnumerableExtensions.cs
├── Objects/
│   ├── ExceptionExtensions.cs
│   ├── GenericExtensions.cs
│   └── ObjectExtensions.cs
├── Streams/
│   └── StreamExtensions.cs
├── Strings/
│   ├── EnumExtensions.cs
│   └── StringExtensions.cs
├── Types/
│   ├── GenericTypeExtensions.cs
│   └── TypeExtensions.cs
└── Xml/
    └── XmlExtensions.cs
```

**Moved to PowerCSharp.Extensions.AspNetCore:**
- Configuration/ConfigurationExtensions.cs
- Net/UriExtensions.cs  
- Http/HttpStatusCodeExtensions.cs
- Http/HttpRequestMessageExtensions.cs

## Extension Categories

### DateTime Extensions

Enhanced date and time operations for common scenarios.

#### Methods

##### GetAge

```csharp
public static int GetAge(this DateTime birthDate)
```

**Purpose**: Calculates age based on birth date.

**Parameters**:
- `birthDate`: The birth date

**Returns**: Age in years

**Examples**:

```csharp
DateTime birthDate = new DateTime(1990, 5, 15);
int age = birthDate.GetAge(); // Returns current age based on today's date

// In a user management system
public class User
{
    public DateTime BirthDate { get; set; }
    
    public int Age => BirthDate.GetAge();
    
    public bool IsAdult => Age >= 18;
}
```

##### IsWeekend

```csharp
public static bool IsWeekend(this DateTime date)
```

**Purpose**: Checks if a date falls on a weekend.

**Parameters**:
- `date`: The date to check

**Returns**: True if Saturday or Sunday, false otherwise

**Examples**:

```csharp
DateTime today = DateTime.Now;
bool isWeekend = today.IsWeekend();

// In a scheduling system
public class TaskScheduler
{
    public bool CanScheduleTask(DateTime date)
    {
        if (date.IsWeekend())
        {
            Console.WriteLine("Task cannot be scheduled on weekend");
            return false;
        }
        return true;
    }
}
```

##### FirstDayOfMonth / LastDayOfMonth

```csharp
public static DateTime FirstDayOfMonth(this DateTime date)
public static DateTime LastDayOfMonth(this DateTime date)
```

**Purpose**: Gets the first or last day of the month for a given date.

**Examples**:

```csharp
DateTime today = DateTime.Now;
DateTime firstDay = today.FirstDayOfMonth();
DateTime lastDay = today.LastDayOfMonth();

// In a reporting system
public class MonthlyReportGenerator
{
    public DateTime GetReportPeriod(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
    
    public List<DailyData> GetMonthData(DateTime date)
    {
        DateTime start = date.FirstDayOfMonth();
        DateTime end = date.LastDayOfMonth();
        
        return GetDataBetweenDates(start, end);
    }
}
```

### String Extensions

Powerful string manipulation and validation methods.

#### Methods

##### ToTitleCase

```csharp
public static string ToTitleCase(this string input)
```

**Purpose**: Converts a string to title case (first letter of each word capitalized).

**Examples**:

```csharp
string text = "hello world";
string titleCase = text.ToTitleCase(); // "Hello World"

// In a data processing system
public class DataFormatter
{
    public string FormatName(string firstName, string lastName)
    {
        return $"{firstName.ToTitleCase()} {lastName.ToTitleCase()}";
    }
    
    public string FormatAddress(string address)
    {
        return address.ToTitleCase();
    }
}
```

##### ToCamelCase

```csharp
public static string ToCamelCase(this string input)
```

**Purpose**: Converts a string to camelCase format.

**Examples**:

```csharp
string text = "HelloWorld";
string camelCase = text.ToCamelCase(); // "helloWorld"

// In a code generation system
public class CodeGenerator
{
    public string GeneratePropertyName(string fieldName)
    {
        return fieldName.ToCamelCase();
    }
    
    public string GenerateVariableName(string description)
    {
        return description.Replace(" ", "").ToCamelCase();
    }
}
```

##### IsValidUrl

```csharp
public static bool IsValidUrl(this string input)
```

**Purpose**: Validates if a string is a valid absolute HTTP or HTTPS URL.

**Examples**:

```csharp
string url1 = "https://example.com";
bool isValid1 = url1.IsValidUrl(); // true

string url2 = "not-a-url";
bool isValid2 = url2.IsValidUrl(); // false

// In a validation system
public class InputValidator
{
    public bool ValidateWebsite(string website)
    {
        return string.IsNullOrEmpty(website) || website.IsValidUrl();
    }
    
    public bool ValidateApiEndpoint(string endpoint)
    {
        return endpoint.IsValidUrl();
    }
}
```

##### IsNullOrWhiteSpace

```csharp
public static bool IsNullOrWhiteSpace(this string input)
```

**Purpose**: Checks if a string is null, empty, or contains only whitespace.

**Examples**:

```csharp
string text = "   ";
bool isEmpty = text.IsNullOrWhiteSpace(); // true

// In a validation system
public class RequiredFieldValidator
{
    public bool ValidateField(string value)
    {
        return !value.IsNullOrWhiteSpace();
    }
    
    public string SanitizeInput(string input)
    {
        return input.IsNullOrWhiteSpace() ? string.Empty : input.Trim();
    }
}
```

### Collection Extensions

Enhanced collection operations for better data manipulation.

#### Methods

##### IsNullOrEmpty

```csharp
public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
```

**Purpose**: Checks if a collection is null or empty.

**Examples**:

```csharp
List<string> items = null;
bool isEmpty = items.IsNullOrEmpty(); // true

items = new List<string>();
isEmpty = items.IsNullOrEmpty(); // true

// In a data processing system
public class DataProcessor
{
    public void ProcessData(IEnumerable<DataRecord> records)
    {
        if (records.IsNullOrEmpty())
        {
            Console.WriteLine("No data to process");
            return;
        }
        
        foreach (var record in records)
        {
            ProcessRecord(record);
        }
    }
}
```

##### FirstOrDefaultSafe

```csharp
public static T FirstOrDefaultSafe<T>(this IEnumerable<T> source, T defaultValue = default)
```

**Purpose**: Safely gets the first element or returns default value.

**Examples**:

```csharp
List<int> numbers = new List<int> { 1, 2, 3 };
int first = numbers.FirstOrDefaultSafe(-1); // 1

List<int> empty = new List<int>();
int firstEmpty = empty.FirstOrDefaultSafe(-1); // -1

// In a query system
public class QueryService
{
    public T GetFirstRecord<T>(IEnumerable<T> records, T defaultValue = default)
    {
        return records.FirstOrDefaultSafe(defaultValue);
    }
    
    public User GetActiveUser(IEnumerable<User> users)
    {
        return users.FirstOrDefault(u => u.IsActive, new User());
    }
}
```

##### Page

```csharp
public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int page, int pageSize)
```

**Purpose**: Gets a specific page of data from a collection.

**Examples**:

```csharp
List<int> numbers = Enumerable.Range(1, 100).ToList();
IEnumerable<int> page1 = numbers.Page(1, 10); // 1-10
IEnumerable<int> page2 = numbers.Page(2, 10); // 11-20

// In a pagination system
public class PaginationService<T>
{
    public PagedResult<T> GetPage(IEnumerable<T> items, int pageNumber, int pageSize)
    {
        var totalCount = items.Count();
        var pagedItems = items.Page(pageNumber, pageSize);
        
        return new PagedResult<T>
        {
            Items = pagedItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
```

##### RemoveAll

```csharp
public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
```

**Purpose**: Removes all elements from an IList that satisfy the specified condition.

**Examples**:

```csharp
List<string> items = new List<string> { "keep", "remove", "keep", "remove" };
int removed = items.RemoveAll(x => x == "remove"); // Returns 2

// In a data cleanup system
public class DataCleaner
{
    public int RemoveInvalidRecords<T>(IList<T> records, Func<T, bool> isValid)
    {
        return records.RemoveAll(item => !isValid(item));
    }
    
    public int RemoveExpiredItems(IList<ExpirableItem> items)
    {
        return items.RemoveAll(item => item.IsExpired);
    }
}
```

### HTTP & Network Extensions

Simplified HTTP status code handling and URL manipulation.

#### Methods

##### HttpStatusCode Extensions

```csharp
public static bool IsSuccessful(this HttpStatusCode statusCode)
public static bool IsClientError(this HttpStatusCode statusCode)
public static bool IsServerError(this HttpStatusCode statusCode)
public static bool IsError(this HttpStatusCode statusCode)
public static bool IsRedirect(this HttpStatusCode statusCode)
public static bool IsCaching(this HttpStatusCode statusCode)
```

**Examples**:

```csharp
HttpStatusCode status = HttpStatusCode.OK;
bool success = status.IsSuccessful(); // true
bool clientError = status.IsClientError(); // false
bool serverError = status.IsServerError(); // false

// In an HTTP client
public class ApiClient
{
    public void HandleResponse(HttpResponseMessage response)
    {
        if (response.StatusCode.IsSuccessful())
        {
            Console.WriteLine("Request successful");
        }
        else if (response.StatusCode.IsClientError())
        {
            Console.WriteLine("Client error - check request parameters");
        }
        else if (response.StatusCode.IsServerError())
        {
            Console.WriteLine("Server error - try again later");
        }
    }
}
```

##### AddParameter (Uri Extensions)

```csharp
public static Uri AddParameter(this Uri url, string parameterName, string parameterValue)
```

**Purpose**: Adds a parameter to the query string of a URI.

**Examples**:

```csharp
Uri baseUri = new Uri("https://api.example.com/users");
Uri withParam = baseUri.AddParameter("page", "1"); // https://api.example.com/users?page=1
Uri withMultiple = withParam.AddParameter("limit", "10"); // https://api.example.com/users?page=1&limit=10

// In an API client
public class ApiQueryBuilder
{
    public Uri BuildQuery(string baseUrl, Dictionary<string, string> parameters)
    {
        Uri uri = new Uri(baseUrl);
        
        foreach (var param in parameters)
        {
            uri = uri.AddParameter(param.Key, param.Value);
        }
        
        return uri;
    }
}
```

##### Clone (HttpRequestMessage Extensions)

```csharp
public static HttpRequestMessage Clone(this HttpRequestMessage original)
public static Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage original, CancellationToken cancellationToken = default)
```

**Purpose**: Creates a deep clone of an HttpRequestMessage.

**Examples**:

```csharp
using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com");
var clonedRequest = request.Clone();

// In a retry mechanism
public class RetryHandler
{
    public async Task<HttpResponseMessage> ExecuteWithRetryAsync(HttpRequestMessage request, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var clonedRequest = await request.CloneAsync();
                var response = await _httpClient.SendAsync(clonedRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                
                response.Dispose();
            }
            catch
            {
                if (i == maxRetries - 1) throw;
            }
            
            await Task.Delay(1000 * (i + 1));
        }
        
        throw new InvalidOperationException("Max retries exceeded");
    }
}
```

### LINQ & Dynamic Query Extensions

Advanced LINQ operations with dynamic expression parsing.

#### Methods

##### GetExpressionDelegate

```csharp
public static Func<T, bool> GetExpressionDelegate<T>(this string stringExpression)
```

**Purpose**: Parses a LINQ expression string into a compiled predicate function.

**Examples**:

```csharp
string expression = "Age > 18 && Name.Contains('John')";
var predicate = expression.GetExpressionDelegate<Person>();

List<Person> people = GetPeople();
var adultsNamedJohn = people.Where(predicate);

// In a dynamic query system
public class DynamicQueryService<T>
{
    public IEnumerable<T> FilterByExpression(IEnumerable<T> items, string expression)
    {
        var predicate = expression.GetExpressionDelegate<T>();
        return items.Where(predicate);
    }
    
    public IEnumerable<T> ApplyUserFilter(IEnumerable<T> items, string userFilter)
    {
        if (string.IsNullOrEmpty(userFilter))
        {
            return items;
        }
        
        return FilterByExpression(items, userFilter);
    }
}
```

##### GetOrderDelegates

```csharp
public static List<(Func<TSource, object>, bool)> GetOrderDelegates<TSource>(this string stringExpression)
```

**Purpose**: Parses a LINQ expression string into a list of ordering delegates.

**Examples**:

```csharp
string orderExpression = "Name DESC, Age ASC";
var orderDelegates = orderExpression.GetOrderDelegates<Person>();

// In a dynamic sorting system
public class DynamicSortService<T>
{
    public IOrderedEnumerable<T> ApplySorting(IEnumerable<T> items, string sortExpression)
    {
        var orderDelegates = sortExpression.GetOrderDelegates<T>();
        
        IOrderedEnumerable<T> ordered = null;
        foreach (var (keySelector, descending) in orderDelegates)
        {
            ordered = ordered == null
                ? descending ? items.OrderByDescending(keySelector) : items.OrderBy(keySelector)
                : descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
        }
        
        return ordered ?? items.OrderBy(x => true);
    }
}
```

##### Filter / Order (IEnumerable Extensions)

```csharp
public static IEnumerable<TSource> Filter<TSource>(this IEnumerable<TSource> source, IDynamicFilterProvider<TSource>? filterProvider)
public static IEnumerable<TSource> Order<TSource>(this IEnumerable<TSource> source, IDynamicOrderProvider<TSource>? orderProvider)
```

**Purpose**: Applies dynamic filtering and ordering using provider interfaces.

**Examples**:

```csharp
// Using with PowerCSharp.Core interfaces
var filterProvider = new DynamicFilterProvider<Person>();
filterProvider.SetFilter(person => person.Age > 18);

var orderProvider = new DynamicOrderProvider<Person>();
orderProvider.SetOrderDelegates(new List<(Func<Person, object>, bool)>
{
    (person => person.Name, false),
    (person => person.Age, true)
});

var people = GetPeople();
var filtered = people.Filter(filterProvider);
var ordered = filtered.Order(orderProvider);

// In a data service
public class DataService<T>
{
    public IEnumerable<T> GetData(IDynamicFilterProvider<T>? filterProvider = null, 
                                 IDynamicOrderProvider<T>? orderProvider = null)
    {
        var data = _repository.GetAll();
        
        if (filterProvider != null)
        {
            data = data.Filter(filterProvider);
        }
        
        if (orderProvider != null)
        {
            data = data.Order(orderProvider);
        }
        
        return data;
    }
}
```

### JSON & XML Extensions

Simplified JSON and XML document manipulation.

#### Methods

##### Get (JsonExtensions)

```csharp
public static JsonElement? Get(this JsonElement element, string name)
public static JsonElement? Get(this JsonElement element, int index)
```

**Purpose**: Safely gets properties or array items from JsonElement.

**Examples**:

```csharp
JsonElement element = JsonDocument.Parse("{\"name\":\"John\",\"age\":30}").RootElement;
var name = element.Get("name"); // JsonElement with value "John"
var age = element.Get("age");   // JsonElement with value 30

JsonArray array = JsonDocument.Parse("[1,2,3]").RootElement;
var first = array.Get(0); // JsonElement with value 1

// In a JSON processing service
public class JsonProcessingService
{
    public string ExtractField(string json, string fieldName)
    {
        var document = JsonDocument.Parse(json);
        var field = document.RootElement.Get(fieldName);
        return field?.ValueToString() ?? string.Empty;
    }
    
    public List<string> ExtractArrayItems(string jsonArray)
    {
        var document = JsonDocument.Parse(jsonArray);
        var items = new List<string>();
        
        for (int i = 0; i < document.RootElement.GetArrayLength(); i++)
        {
            var item = document.RootElement.Get(i);
            items.Add(item?.ValueToString() ?? string.Empty);
        }
        
        return items;
    }
}
```

##### TryGetPropertyCaseInsensitive

```csharp
public static bool TryGetPropertyCaseInsensitive(this JsonElement element, string propertyName, out JsonElement value)
```

**Purpose**: Looks for a property using case-insensitive comparison.

**Examples**:

```csharp
JsonElement element = JsonDocument.Parse("{\"NAME\":\"John\",\"AGE\":30}").RootElement;
bool found = element.TryGetPropertyCaseInsensitive("name", out var name); // true
bool foundAge = element.TryGetPropertyCaseInsensitive("age", out var age); // true

// In a flexible JSON parser
public class FlexibleJsonParser
{
    public Dictionary<string, object> ParseToObject(string json)
    {
        var document = JsonDocument.Parse(json);
        var result = new Dictionary<string, object>();
        
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (element.TryGetPropertyCaseInsensitive(property.Name, out var value))
            {
                result[property.Name] = ExtractValue(value);
            }
        }
        
        return result;
    }
    
    private object ExtractValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }
}
```

##### Flatten (XmlExtensions)

```csharp
public static Dictionary<string, object> Flatten(this XElement xmlElement)
```

**Purpose**: Converts XML element to a flat dictionary representation.

**Examples**:

```csharp
XElement xml = XElement.Parse("<root><child>value</child><attr>test</attr></root>");
var dict = xml.Flatten();
// Dictionary contains XML structure with attributes and nested elements

// In an XML processing service
public class XmlProcessingService
{
    public Dictionary<string, object> ConvertXmlToDictionary(string xml)
    {
        var element = XElement.Parse(xml);
        return element.Flatten();
    }
    
    public void ProcessXmlConfiguration(string xmlConfig)
    {
        var configDict = ConvertXmlToDictionary(xmlConfig);
        
        foreach (var setting in configDict)
        {
            Console.WriteLine($"{setting.Key}: {setting.Value}");
        }
    }
}
```

### Object & Type Extensions

Enhanced object manipulation and type operations.

#### Methods

##### ThrowOnNull

```csharp
public static T ThrowOnNull<T>(this T? value) where T : class
```

**Purpose**: Throws ArgumentNullException if the value is null.

**Examples**:

```csharp
string text = null;
text.ThrowOnNull(); // Throws ArgumentNullException

// In a validation system
public class ParameterValidator
{
    public void ValidateParameters(string name, object data)
    {
        name.ThrowOnNull();
        data.ThrowOnNull();
        
        // Continue with validation
    }
}
```

##### CopyPropertiesTo

```csharp
public static void CopyPropertiesTo<TSource, TDestination>(this TSource source, TDestination destination)
    where TSource : class
    where TDestination : class
```

**Purpose**: Copies all matching properties by name and type from source to destination.

**Examples**:

```csharp
var source = new Person { Name = "John", Age = 30 };
var destination = new Person();
source.CopyPropertiesTo(destination);

// In a data mapping service
public class MappingService
{
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        where TSource : class
        where TDestination : class
    {
        source.ThrowOnNull();
        destination.ThrowOnNull();
        
        source.CopyPropertiesTo(destination);
        return destination;
    }
    
    public TDestination CreateMapped<TSource, TDestination>(TSource source)
        where TSource : class
        where TDestination : class, new()
    {
        var destination = new TDestination();
        return Map(source, destination);
    }
}
```

##### GetConcreteType

```csharp
public static Type? GetConcreteType(this Type interfaceType)
```

**Purpose**: Returns the concrete Type that implements the specified interface type.

**Examples**:

```csharp
Type concreteType = typeof(IMyInterface).GetConcreteType();

// In a plugin system
public class PluginLoader
{
    public IEnumerable<Type> LoadPluginTypes<TInterface>()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(TInterface).IsAssignableFrom(type) && !type.IsInterface)
            .ToList();
    }
    
    public TInterface CreatePlugin<TInterface>(string typeName)
    {
        var interfaceType = typeof(TInterface);
        var concreteType = interfaceType.GetConcreteType();
        
        if (concreteType?.FullName == typeName)
        {
            return (TInterface)Activator.CreateInstance(concreteType);
        }
        
        throw new InvalidOperationException($"Plugin {typeName} not found");
    }
}
```

## Advanced Usage Patterns

### Web API Integration

```csharp
public class EnhancedApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        if (result.ErrorType == ErrorType.NotFound)
        {
            return NotFound(result.ErrorMessage);
        }
        
        if (result.ErrorType == ErrorType.Validation)
        {
            return BadRequest(result.ErrorMessage);
        }
        
        return StatusCode(500, result.ErrorMessage);
    }
    
    protected IActionResult HandlePagedResult<T>(PagedResult<T> result)
    {
        if (result.Items.IsNullOrEmpty())
        {
            return Ok(new { items = Array.Empty<T>(), totalCount = 0, pageNumber = result.PageNumber, pageSize = result.PageSize });
        }
        
        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize,
            totalPages = result.TotalPages
        });
    }
}
```

### Data Processing Pipeline

```csharp
public class DataProcessingPipeline<T>
{
    private readonly List<Func<IEnumerable<T>, IEnumerable<T>>> _processors = new();
    
    public DataProcessingPipeline<T> AddProcessor(Func<IEnumerable<T>, IEnumerable<T>> processor)
    {
        _processors.Add(processor);
        return this;
    }
    
    public DataProcessingPipeline<T> Filter(Func<T, bool> predicate)
    {
        return AddProcessor(items => items.Where(predicate));
    }
    
    public DataProcessingPipeline<T> Transform(Func<T, T> transformer)
    {
        return AddProcessor(items => items.Select(transformer));
    }
    
    public DataProcessingPipeline<T> Sort(Func<T, IComparable> keySelector)
    {
        return AddProcessor(items => items.OrderBy(keySelector));
    }
    
    public DataProcessingPipeline<T> Page(int pageNumber, int pageSize)
    {
        return AddProcessor(items => items.Page(pageNumber, pageSize));
    }
    
    public IEnumerable<T> Execute(IEnumerable<T> source)
    {
        var result = source;
        
        foreach (var processor in _processors)
        {
            result = processor(result);
        }
        
        return result;
    }
}
```

### Configuration Management

```csharp
public class EnhancedConfigurationManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EnhancedConfigurationManager> _logger;
    
    public EnhancedConfigurationManager(IConfiguration configuration, ILogger<EnhancedConfigurationManager> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public T GetOptions<T>(string sectionName) where T : class, IAppOptions, new()
    {
        try
        {
            var options = _configuration.GetOptions<T>(sectionName);
            
            if (options == null)
            {
                _logger.LogWarning("Configuration section {SectionName} not found, using defaults", sectionName);
                return new T();
            }
            
            ValidateOptions(options);
            return options;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration section {SectionName}", sectionName);
            return new T();
        }
    }
    
    private void ValidateOptions(IAppOptions options)
    {
        if (options is IValidatableOptions validatable)
        {
            validatable.Validate();
        }
    }
}
```

## Performance Considerations

### Extension Method Performance

1. **Avoid Repeated Enumerations**: Cache enumerated results when used multiple times
2. **Use StringBuilder**: For string operations that concatenate multiple strings
3. **LINQ Optimization**: Use appropriate LINQ methods for specific scenarios
4. **Memory Allocation**: Be aware of allocations in hot paths

### Best Practices

1. **Null Checking**: Always check for null before calling extension methods
2. **Error Handling**: Provide meaningful error messages for invalid operations
3. **Documentation**: Include comprehensive XML documentation for custom extensions
4. **Testing**: Test edge cases and boundary conditions

## Version History

### v0.2.0
- Restructured for cross-platform compatibility
- Moved ASP.NET Core specific extensions to PowerCSharp.Extensions.AspNetCore
- Enhanced .NET Standard 2.0 compatibility
- Updated dependency management for cross-platform support

### v0.1.0
- Initial release with comprehensive extension methods
- String manipulation and validation extensions
- Collection and LINQ enhancements
- JSON and XML processing extensions
- Object and type manipulation utilities
- Stream and exception handling extensions
- Performance-optimized implementations

## Future Enhancements

### Planned Extensions

- **Async Extensions**: Async versions of synchronous methods
- **Validation Extensions**: Enhanced validation patterns
- **Caching Extensions**: Built-in caching utilities
- **Logging Extensions**: Structured logging utilities

### Planned Features

- **Expression Tree Extensions**: Advanced expression tree manipulation
- **Reflection Extensions**: Enhanced reflection utilities
- **Parallel Extensions**: Parallel processing utilities
- **Memory Extensions**: Memory pool and allocation utilities

---

For more information, see:
- [PowerCSharp.Extensions README](../src/PowerCSharp.Extensions/README.md)
- [PowerCSharp.Core Documentation](PowerCSharp.Core.md)
- [Main PowerCSharp Documentation](../README.md)
