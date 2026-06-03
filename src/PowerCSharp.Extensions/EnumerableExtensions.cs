using System;
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

    /// <summary>
    /// Filters a sequence of values based on a predicate function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">The source collection to filter.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>An IEnumerable of TSource that contains elements from the input sequence that satisfy the condition.</returns>
    public static IEnumerable<TSource> Filter<TSource>(this IEnumerable<TSource> source, Func<TSource, bool>? predicate = null)
    {
        if (source == null)
        {
            return source;
        }

        var filter = predicate ?? (_ => true);
        var result = source.Where(filter);
        return result;
    }

    /// <summary>
    /// Sorts the elements of a sequence according to a key selector.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <param name="source">The source collection to order.</param>
    /// <param name="keySelector">A function to extract a key from an element.</param>
    /// <param name="descending">Whether to sort in descending order.</param>
    /// <returns>An IOrderedEnumerable whose elements are sorted according to the key.</returns>
    public static IOrderedEnumerable<TSource> Order<TSource, TKey>(
        this IEnumerable<TSource> source, 
        Func<TSource, TKey> keySelector, 
        bool descending = false)
    {
        if (source == null)
        {
            return source as IOrderedEnumerable<TSource> ?? Enumerable.Empty<TSource>().OrderBy(x => default(TKey)!);
        }

        return descending 
            ? source.OrderByDescending(keySelector) 
            : source.OrderBy(keySelector);
    }
}
