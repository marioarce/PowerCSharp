# PowerCSharp.Extensions API Documentation

This document provides comprehensive API documentation for all extension methods in the PowerCSharp.Extensions package.

## Table of Contents

- [Collections](#collections)
- [Configuration](#configuration)
- [HTTP & Network](#http--network)
- [JSON & XML](#json--xml)
- [LINQ & Dynamic Queries](#linq--dynamic-queries)
- [Objects & Types](#objects--types)
- [Streams](#streams)
- [Strings](#strings)
- [Interfaces](#interfaces)

## Collections

### IListExtensions

#### RemoveAll<T>

Removes all elements from an IList that satisfy the specified condition.

```csharp
public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
```

**Parameters:**
- `list`: The IList to remove elements from
- `match`: The predicate condition to match elements for removal

**Returns:** The number of elements removed from the list

**Example:**
```csharp
var list = new List<string> { "keep", "remove", "keep", "remove" };
int removed = list.RemoveAll(x => x == "remove"); // Returns 2
```

## Configuration

### ConfigurationExtensions

#### GetOptions<TOptions>

Gets options from IConfiguration object using the specified configuration section path.

```csharp
public static TOptions GetOptions<TOptions>(this IConfiguration configuration, string sectionPath)
    where TOptions : class, IAppOptions
```

**Parameters:**
- `configuration`: The IConfiguration object to read from
- `sectionPath`: The configuration section path to use

**Returns:** The configured options object

**Exceptions:**
- `ArgumentNullException`: When configuration is null
- `ArgumentException`: When sectionPath is null or whitespace
- `InvalidOperationException`: When binding fails

**Example:**
```csharp
var options = configuration.GetOptions<MyAppOptions>("MyApp");
```

## HTTP & Network

### HttpStatusCodeExtensions

#### IsSuccessful

Checks if the HTTP status code is successful (2xx range).

```csharp
public static bool IsSuccessful(this HttpStatusCode statusCode)
public static bool IsSuccessful(this int statusCode)
```

#### IsClientError

Checks if the HTTP status code is a client error (4xx range).

```csharp
public static bool IsClientError(this HttpStatusCode statusCode)
public static bool IsClientError(this int statusCode)
```

#### IsServerError

Checks if the HTTP status code is a server error (5xx range).

```csharp
public static bool IsServerError(this HttpStatusCode statusCode)
public static bool IsServerError(this int statusCode)
```

#### IsError

Checks if the HTTP status code is an error (4xx or 5xx range).

```csharp
public static bool IsError(this HttpStatusCode statusCode)
public static bool IsError(this int statusCode)
```

#### IsRedirect

Checks if the HTTP status code is in the redirect category (3xx range).

```csharp
public static bool IsRedirect(this HttpStatusCode statusCode)
public static bool IsRedirect(this int statusCode)
```

#### IsCaching

Checks if the HTTP status code indicates caching (NotModified).

```csharp
public static bool IsCaching(this HttpStatusCode statusCode)
```

**Example:**
```csharp
HttpStatusCode status = HttpStatusCode.OK;
bool success = status.IsSuccessful(); // true
bool clientError = status.IsClientError(); // false
```

### UriExtensions

#### AddParameter

Adds the specified parameter to the query string of the URI.

```csharp
public static Uri AddParameter(this Uri url, string parameterName, string parameterValue)
```

**Parameters:**
- `url`: The base URI to add the parameter to
- `parameterName`: The name of the parameter to add
- `parameterValue`: The value for the parameter to add

**Returns:** A new URI with the added parameter in the query string

**Example:**
```csharp
Uri uri = new Uri("https://example.com");
Uri withParam = uri.AddParameter("search", "test");
// Result: https://example.com?search=test
```

### HttpRequestMessageExtensions

#### Clone

Creates a deep clone of the HttpRequestMessage including headers, content, and properties.

```csharp
public static HttpRequestMessage Clone(this HttpRequestMessage original)
```

#### CloneAsync

Creates a deep clone of the HttpRequestMessage asynchronously.

```csharp
public static Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage original, CancellationToken cancellationToken = default)
```

**Parameters:**
- `original`: The original HttpRequestMessage to clone
- `cancellationToken`: A cancellation token to cancel the operation

**Returns:** A new HttpRequestMessage instance with all headers, content, and properties copied

**Example:**
```csharp
using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com");
var clonedRequest = request.Clone();
var clonedAsync = await request.CloneAsync();
```

## JSON & XML

### JsonExtensions

#### Get (string)

Looks for a property with the specified name in the current JSON element.

```csharp
public static JsonElement? Get(this JsonElement element, string name)
```

#### Get (int)

Looks for a property by index in the current JSON array element.

```csharp
public static JsonElement? Get(this JsonElement element, int index)
```

**Parameters:**
- `element`: The JSON element to search within
- `name`: The name of the property to find
- `index`: The zero-based index of the element to retrieve

**Returns:** The JsonElement value if found and valid; otherwise, null

**Example:**
```csharp
JsonElement element = JsonDocument.Parse("{\"name\":\"John\"}").RootElement;
var name = element.Get("name"); // JsonElement with value "John"
```

### JsonElementExtensions

#### TryGetPropertyCaseInsensitive

Looks for a property with the specified name using case-insensitive comparison.

```csharp
public static bool TryGetPropertyCaseInsensitive(this JsonElement element, string propertyName, out JsonElement value)
```

**Parameters:**
- `element`: The JSON element to search within
- `propertyName`: The name of the property to find (case-insensitive)
- `value`: When this method returns, contains the JsonElement value if the property was found

**Returns:** True if the property exists; otherwise, false

**Example:**
```csharp
bool found = element.TryGetPropertyCaseInsensitive("NAME", out var value);
```

### XmlExtensions

#### Flatten

Gets the XElement transformed into a Dictionary representation.

```csharp
public static Dictionary<string, object> Flatten(this XElement xmlElement)
```

**Parameters:**
- `xmlElement`: The XML element to flatten

**Returns:** A dictionary representation of the XML element with attributes and nested elements

**Example:**
```csharp
XElement xml = XElement.Parse("<root><child>value</child></root>");
var dict = xml.Flatten(); // Dictionary with XML structure
```

## LINQ & Dynamic Queries

### DynamicExpressionExtensions

#### GetExpressionDelegate<T>

Parses a LINQ expression string into a compiled predicate function.

```csharp
public static Func<T, bool> GetExpressionDelegate<T>(this string stringExpression)
```

#### GetOrderDelegates<TSource>

Parses a LINQ expression string into a list of ordering delegates.

```csharp
public static List<(Func<TSource, object>, bool)> GetOrderDelegates<TSource>(this string stringExpression)
```

**Parameters:**
- `stringExpression`: The string expression to parse

**Returns:** A predicate function or list of ordering delegates

**Example:**
```csharp
string expression = "Age > 18 && Name.Contains('John')";
var predicate = expression.GetExpressionDelegate<Person>();

string orderExpression = "Name DESC, Age ASC";
var orderDelegates = orderExpression.GetOrderDelegates<Person>();
```

### IEnumerableExtensions

#### Filter<TSource>

Filters a sequence of values based on a dynamic filter provider.

```csharp
public static IEnumerable<TSource> Filter<TSource>(this IEnumerable<TSource> source, IDynamicFilterProvider<TSource>? filterProvider)
```

#### Order<TSource>

Sorts the elements of a sequence in ascending or descending order based on a dynamic order provider.

```csharp
public static IEnumerable<TSource> Order<TSource>(this IEnumerable<TSource> source, IDynamicOrderProvider<TSource>? orderProvider)
```

**Parameters:**
- `source`: The source sequence to filter/sort
- `filterProvider`: The dynamic filter provider containing filter criteria
- `orderProvider`: The dynamic order provider containing ordering criteria

**Returns:** An IEnumerable with elements filtered or sorted

**Example:**
```csharp
var filterProvider = new DynamicFilterProvider<Person>();
var orderProvider = new DynamicOrderProvider<Person>();
var filtered = people.Filter(filterProvider);
var ordered = people.Order(orderProvider);
```

## Objects & Types

### GenericExtensions

#### FromHierarchy<TSource>

Processes a structure hierarchically using the provided next item function and continuation condition.

```csharp
public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem, Func<TSource, bool> canContinue)
public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem)
    where TSource : class
```

#### CopyPropertiesTo<TSource, TDestination>

Copies all matching properties by name and type from the source object to the destination object.

```csharp
public static void CopyPropertiesTo<TSource, TDestination>(this TSource source, TDestination destination)
    where TSource : class
    where TDestination : class
```

**Example:**
```csharp
var person = new Person { Name = "John", Age = 30 };
var copy = new Person();
person.CopyPropertiesTo(copy); // Copies matching properties
```

### ObjectExtensions

#### ThrowOnNull<T>

Throws an ArgumentNullException if the value is null.

```csharp
public static T ThrowOnNull<T>(this T? value)
    where T : class
```

#### TryGetBool

Attempts to convert various types of input into a boolean value.

```csharp
public static bool TryGetBool(this object value, out bool result)
```

#### Map<TDATA>

Maps properties from the source object to a new destination object of type TDATA.

```csharp
public static TDATA? Map<TDATA>(this object oldObject)
    where TDATA : class, new()
```

**Example:**
```csharp
string text = "test";
text.ThrowOnNull(); // Throws if null

bool isTrue = "true".TryGetBool(out bool result); // result = true, isTrue = true
```

### GenericTypeExtensions

#### IsDefault<T>

Check if the value is equal to its default value.

```csharp
public static bool IsDefault<T>(this T value)
```

#### GetGenericTypeName

Returns the name of the generic type of the object, including generic parameters.

```csharp
public static string GetGenericTypeName(this object @object)
```

**Example:**
```csharp
bool isDefault = default(int).IsDefault(); // true
string typeName = typeof(List<string>).GetGenericTypeName(); // "List<String>"
```

### TypeExtensions

#### GetConcreteType

Returns the concrete Type that implements the specified interface type.

```csharp
public static Type? GetConcreteType(this Type interfaceType)
```

**Example:**
```csharp
Type concreteType = typeof(IMyInterface).GetConcreteType();
```

## Streams

### StreamExtensions

#### CloneAsync

Asynchronously clones the content of this stream to another stream.

```csharp
public static Task CloneAsync(this Stream stream, Stream? destination)
```

**Parameters:**
- `stream`: The source stream to clone
- `destination`: The destination stream. If null, a new MemoryStream will be created

**Example:**
```csharp
using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes("test data"));
using var destinationStream = new MemoryStream();
await originalStream.CloneAsync(destinationStream);
```

## Strings

### StringExtensions

#### Mid

Gets the middle part of a string starting from the specified index.

```csharp
public static string Mid(this string text, int start)
```

#### FirstCharToLowerCase

Gets the string with the first character converted to lowercase.

```csharp
public static string? FirstCharToLowerCase(this string? input)
```

#### ToCamelCase

Converts the string to camel case format.

```csharp
public static string ToCamelCase(this string input)
```

#### NormalizeKey

Normalizes a string to be a valid JSON key.

```csharp
public static string NormalizeKey(this string input)
```

#### AsAscii

Strips non-ASCII characters from the string.

```csharp
public static string AsAscii(this string input)
```

#### IsValidUrl

Returns true if the string is a valid absolute HTTP or HTTPS URL.

```csharp
public static bool IsValidUrl(this string input)
```

**Example:**
```csharp
string camel = "HelloWorld".ToCamelCase(); // "helloWorld"
string firstLower = text.FirstCharToLowerCase(); // "hello world"
string normalized = "User Name".NormalizeKey(); // "userName"
bool isValid = "https://example.com".IsValidUrl(); // true
```

## Interfaces

### IDynamicFilterProvider<T>

Implement a Service Provider for dynamically filtering of the type T using Dynamic Expressions.

```csharp
public interface IDynamicFilterProvider<T>
{
    void SetFilter(Func<T, bool> filter);
    Func<T, bool> GetFilter();
}
```

### IDynamicOrderProvider<T>

Implement a Service Provider for dynamically ordering of the type T using Dynamic Expressions.

```csharp
public interface IDynamicOrderProvider<T>
{
    void SetOrderDelegates(List<(Func<T, object>, bool)> delegates);
    List<(Func<T, object>, bool)> GetOrderDelegates();
}
```

### IAppOptions

Represents the interface for application options.

```csharp
public interface IAppOptions
{
    string ConfigSectionPath { get; }
}
```

## Framework Support

- **.NET 8.0** - Full support with all features
- **.NET Standard 2.0** - Compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+

## Dependencies

The PowerCSharp.Extensions package requires the following NuGet packages:

- **System.Linq.Dynamic.Core** - For dynamic LINQ expression parsing
- **Microsoft.AspNetCore.WebUtilities** - For URL query string manipulation
- **Microsoft.Extensions.Configuration.Abstractions** - For configuration support
- **Microsoft.Extensions.Configuration.Binder** - For configuration binding
- **System.Text.Json** - For JSON processing

## Thread Safety

All extension methods in PowerCSharp.Extensions are designed to be thread-safe when used with immutable data structures. For mutable collections, proper synchronization should be maintained by the calling code.

## Performance Considerations

- **Dynamic LINQ extensions** involve runtime compilation and may have performance overhead compared to compiled LINQ queries
- **String manipulation methods** create new string instances and should be used judiciously in performance-critical code
- **Stream cloning** creates copies of stream data and should be used with awareness of memory implications

## Error Handling

All extension methods include comprehensive null checking and graceful error handling:
- ArgumentNullException is thrown for null parameters where applicable
- InvalidOperationException is thrown for invalid operations
- Methods return default values or empty collections when appropriate rather than throwing exceptions

For more detailed information about specific methods, refer to the XML documentation included in the source code.
