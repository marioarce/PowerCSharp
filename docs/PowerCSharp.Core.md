# PowerCSharp.Core - Detailed Documentation

## Overview

PowerCSharp.Core serves as the foundational core of the PowerCSharp ecosystem, providing centralized interfaces, models, and base functionality for all PowerCSharp packages.

## Architecture

### Centralized Interface Design

All interfaces in PowerCSharp are centralized in the Core package to maintain:

- **Single source of truth** for contracts and abstractions
- **Consistent namespace organization** across packages
- **Easy dependency management** across the ecosystem
- **Clear architectural boundaries** between packages

### Namespace Structure

```
PowerCSharp.Core/
├── Interfaces/
│   ├── Extensions/
│   │   ├── Configuration/
│   │   │   └── IAppOptions.cs
│   │   └── Linq/
│   │       ├── IDynamicFilterProvider.cs
│   │       └── IDynamicOrderProvider.cs
│   └── Models/
│       (reserved for future model classes)
```

## Interface Documentation

### IAppOptions

**Namespace:** `PowerCSharp.Core.Interfaces.Extensions.Configuration`

Represents the interface for application options, providing a standardized way to define configuration sections.

```csharp
public interface IAppOptions
{
    /// <summary>
    /// The configuration section path.
    /// </summary>
    string ConfigSectionPath { get; }
}
```

#### Implementation Guidelines

When implementing `IAppOptions`:

1. **Return a meaningful configuration path** that matches your appsettings.json structure
2. **Use consistent naming** for configuration sections
3. **Include validation** for required configuration properties

#### Example Implementation

```csharp
public class DatabaseOptions : IAppOptions
{
    public string ConfigSectionPath => "Database";
    
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }
    public bool EnableRetry { get; set; }
    
    public void Validate()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw new InvalidOperationException("Database connection string is required");
        
        if (Timeout <= 0)
            throw new InvalidOperationException("Database timeout must be greater than 0");
    }
}
```

#### Usage in Configuration Extensions

```csharp
using Microsoft.Extensions.Configuration;
using PowerCSharp.Extensions.Configuration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetOptions<DatabaseOptions>("Database");
        dbOptions.Validate();
        
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
    }
}
```

### IDynamicFilterProvider<T>

**Namespace:** `PowerCSharp.Core.Interfaces.Extensions.Linq`

Provides a flexible way to define dynamic filtering criteria for collections using compiled predicates.

```csharp
public interface IDynamicFilterProvider<T>
{
    /// <summary>
    /// Sets delegate to use to filter
    /// </summary>
    /// <param name="filter">A predicate that takes an object of type T and returns a bool.</param>
    void SetFilter(Func<T, bool> filter);

    /// <summary>
    /// Gets the delegate to be used to filter the T
    /// </summary>
    /// <returns>A predicate that takes an object of type T and returns a bool.</returns>
    Func<T, bool> GetFilter();
}
```

#### Implementation Guidelines

When implementing `IDynamicFilterProvider<T>`:

1. **Handle null filters** by providing a default predicate that returns true
2. **Consider thread safety** if the provider will be used in multi-threaded scenarios
3. **Provide clear error messages** for invalid filter expressions

#### Example Implementation

```csharp
public class DynamicFilterProvider<T> : IDynamicFilterProvider<T>
{
    private Func<T, bool> _filter;
    private readonly object _lock = new object();

    public void SetFilter(Func<T, bool> filter)
    {
        lock (_lock)
        {
            _filter = filter ?? (entity => true);
        }
    }

    public Func<T, bool> GetFilter()
    {
        lock (_lock)
        {
            return _filter ?? (entity => true);
        }
    }
}
```

#### Advanced Usage with Expression Parsing

```csharp
public class ExpressionFilterProvider<T> : IDynamicFilterProvider<T>
{
    private Func<T, bool> _filter;

    public void SetFilter(Func<T, bool> filter)
    {
        _filter = filter ?? (entity => true);
    }

    public Func<T, bool> GetFilter()
    {
        return _filter ?? (entity => true);
    }

    public void SetFilterFromString(string expression)
    {
        try
        {
            var lambdaExpression = DynamicExpressionParser.ParseLambda(
                typeof(T),
                typeof(bool),
                expression);

            _filter = (Func<T, bool>)lambdaExpression.Compile();
        }
        catch
        {
            // If parsing fails, use a filter that includes all entities
            _filter = entity => true;
        }
    }
}
```

### IDynamicOrderProvider<T>

**Namespace:** `PowerCSharp.Core.Interfaces.Extensions.Linq`

Enables dynamic ordering of collections using multiple sorting criteria with ascending/descending options.

```csharp
public interface IDynamicOrderProvider<T>
{
    /// <summary>
    /// Sets delegate to use for ordering
    /// </summary>
    /// <param name="delegates">A list of predicates that takes an object of type T and returns an object.</param>
    void SetOrderDelegates(List<(Func<T, object>, bool)> delegates);

    /// <summary>
    /// Gets the list of delegates to be used to order the T
    /// </summary>
    /// <returns>A list of predicates that takes an object of type T and returns an object.</returns>
    List<(Func<T, object>, bool)> GetOrderDelegates();
}
```

#### Implementation Guidelines

When implementing `IDynamicOrderProvider<T>`:

1. **Handle null delegate lists** by returning an empty list
2. **Validate delegate functions** before adding them to the list
3. **Consider thread safety** for concurrent access
4. **Provide clear error handling** for invalid property access

#### Example Implementation

```csharp
public class DynamicOrderProvider<T> : IDynamicOrderProvider<T>
{
    private List<(Func<T, object>, bool)> _orderDelegates;
    private readonly object _lock = new object();

    public void SetOrderDelegates(List<(Func<T, object>, bool)> delegates)
    {
        lock (_lock)
        {
            _orderDelegates = delegates ?? new List<(Func<T, object>, bool)>();
        }
    }

    public List<(Func<T, object>, bool)> GetOrderDelegates()
    {
        lock (_lock)
        {
            return _orderDelegates ?? new List<(Func<T, object>, bool)>();
        }
    }

    public void AddOrderDelegate(Func<T, object> keySelector, bool descending = false)
    {
        if (keySelector == null) return;

        lock (_lock)
        {
            _orderDelegates ??= new List<(Func<T, object>, bool)>();
            _orderDelegates.Add((keySelector, descending));
        }
    }

    public void ClearOrderDelegates()
    {
        lock (_lock)
        {
            _orderDelegates?.Clear();
        }
    }
}
```

#### Advanced Usage with Expression Parsing

```csharp
public class ExpressionOrderProvider<T> : IDynamicOrderProvider<T>
{
    private List<(Func<T, object>, bool)> _orderDelegates;

    public void SetOrderDelegates(List<(Func<T, object>, bool)> delegates)
    {
        _orderDelegates = delegates ?? new List<(Func<T, object>, bool)>();
    }

    public List<(Func<T, object>, bool)> GetOrderDelegates()
    {
        return _orderDelegates ?? new List<(Func<T, object), bool)>();
    }

    public void SetOrderFromString(string orderExpression)
    {
        var delegates = new List<(Func<T, object>, bool)>();
        
        if (string.IsNullOrEmpty(orderExpression))
        {
            SetOrderDelegates(delegates);
            return;
        }

        var clauses = orderExpression.Split(',');
        foreach (var clause in clauses)
        {
            var parts = clause.Trim().Split(' ');
            if (parts.Length == 0) continue;

            string propertyName = parts[0];
            bool descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

            try
            {
                var keySelector = CreateKeySelector(propertyName);
                delegates.Add((keySelector, descending));
            }
            catch
            {
                // Skip invalid property names
                continue;
            }
        }

        SetOrderDelegates(delegates);
    }

    private Func<T, object> CreateKeySelector(string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(param, propertyName);
        var converted = Expression.Convert(property, typeof(object));

        var lambda = Expression.Lambda<Func<T, object>>(converted, param);
        return lambda.Compile();
    }
}
```

## Integration Patterns

### Combining Filter and Order Providers

```csharp
public class QueryBuilder<T>
{
    private readonly IDynamicFilterProvider<T> _filterProvider;
    private readonly IDynamicOrderProvider<T> _orderProvider;

    public QueryBuilder(
        IDynamicFilterProvider<T> filterProvider,
        IDynamicOrderProvider<T> orderProvider)
    {
        _filterProvider = filterProvider;
        _orderProvider = orderProvider;
    }

    public QueryBuilder<T> Where(string filterExpression)
    {
        if (_filterProvider is ExpressionFilterProvider<T> expressionFilter)
        {
            expressionFilter.SetFilterFromString(filterExpression);
        }
        return this;
    }

    public QueryBuilder<T> OrderBy(string orderExpression)
    {
        if (_orderProvider is ExpressionOrderProvider<T> expressionOrder)
        {
            expressionOrder.SetOrderFromString(orderExpression);
        }
        return this;
    }

    public IEnumerable<T> Execute(IEnumerable<T> source)
    {
        var filtered = source.Where(_filterProvider.GetFilter());
        var ordered = ApplyOrdering(filtered);
        return ordered;
    }

    private IEnumerable<T> ApplyOrdering(IEnumerable<T> source)
    {
        var orderDelegates = _orderProvider.GetOrderDelegates();
        if (orderDelegates == null || orderDelegates.Count == 0)
            return source;

        IOrderedEnumerable<T> ordered = null;
        foreach (var (keySelector, descending) in orderDelegates)
        {
            ordered = ordered == null
                ? descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector)
                : descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
        }

        return ordered ?? source;
    }
}
```

### Configuration Integration

```csharp
public class ConfigurationManager
{
    private readonly IConfiguration _configuration;

    public ConfigurationManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public T GetOptions<T>() where T : class, IAppOptions
    {
        var options = _configuration.GetOptions<T>(typeof(T).Name);
        
        // Validate if the options type has a Validate method
        if (options is IValidatableOptions validatable)
        {
            validatable.Validate();
        }

        return options;
    }
}

public interface IValidatableOptions
{
    void Validate();
}

public class DatabaseOptions : IAppOptions, IValidatableOptions
{
    public string ConfigSectionPath => "Database";
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("Database connection string is required");
        
        if (Timeout <= 0)
            throw new InvalidOperationException("Database timeout must be greater than 0");
    }
}
```

## Best Practices

### Interface Design Principles

1. **Single Responsibility**: Each interface should have one clear purpose
2. **Extensibility**: Design interfaces that can be extended without breaking changes
3. **Testability**: Interfaces should be easy to mock and test
4. **Documentation**: Provide comprehensive XML documentation for all members

### Implementation Guidelines

1. **Null Safety**: Always handle null parameters gracefully
2. **Thread Safety**: Consider thread safety for shared providers
3. **Error Handling**: Provide meaningful error messages
4. **Performance**: Avoid unnecessary allocations and computations

### Usage Patterns

1. **Dependency Injection**: Register implementations in DI containers
2. **Factory Pattern**: Use factories to create configured providers
3. **Builder Pattern**: Use builders for complex query construction
4. **Strategy Pattern**: Switch implementations based on requirements

## Version History

### v0.1.0
- Initial release with centralized interfaces
- IAppOptions for configuration management
- IDynamicFilterProvider<T> for dynamic filtering
- IDynamicOrderProvider<T> for dynamic ordering
- Comprehensive documentation and examples

## Future Enhancements

### Planned Interfaces
- **IValidator<T>** - Standardized validation interface
- **ICacheProvider** - Caching abstraction
- **ILoggerProvider** - Logging abstraction
- **IMapper<TSource, TDestination>** - Object mapping interface

### Planned Models
- **Result<T>** - Operation result wrapper
- **PagedResult<T>** - Paginated query results
- **ApiError** - Standardized error model
- **AuditEvent** - Audit logging model

---

For more information, see:
- [PowerCSharp.Core README](../src/PowerCSharp.Core/README.md)
- [PowerCSharp.Extensions API Documentation](PowerCSharp.Extensions-API.md)
- [Main PowerCSharp Documentation](../README.md)
