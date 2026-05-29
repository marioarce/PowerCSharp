using System.Collections.Generic;
using System.Linq;

namespace PowerCSharp.Extensions;

/// <summary>
/// Extension methods for common .NET collections and enumerables
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Safely gets the first element or returns a default value
    /// </summary>
    public static T? FirstOrDefaultSafe<T>(this IEnumerable<T> source, T? defaultValue = default)
    {
        if (source == null)
        {
            return defaultValue;
        }
        
        using var enumerator = source.GetEnumerator();
        var result = enumerator.MoveNext()
            ? enumerator.Current
            : defaultValue;

        return result;
    }

    /// <summary>
    /// Checks if a collection is null or empty
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        var result = source == null || !source.Any();
        return result;
    }

    /// <summary>
    /// Paginates a collection
    /// </summary>
    public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int page, int pageSize)
    {
        if (source == null || page < 1 || pageSize < 1)
        {
            return Enumerable.Empty<T>();
        }

        var result = source
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return result;
    }
}
