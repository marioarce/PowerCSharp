using System;
using System.Collections.Generic;
using System.Linq;
using PowerCSharp.Core.Interfaces.Extensions.Linq;

namespace PowerCSharp.Extensions.Collections;

/// <summary>
/// Extension methods for IEnumerable collections with dynamic filtering and ordering capabilities
/// </summary>
public static class DynamicEnumerableExtensions
{
    /// <summary>
    /// Filters a sequence of values based on a dynamic filter provider.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity to apply the dynamic filter to.</typeparam>
    /// <param name="source">The source sequence to filter.</param>
    /// <param name="filterProvider">The dynamic filter provider containing filter criteria.</param>
    /// <returns>An IEnumerable of <typeparamref name="TSource"/> that contains elements from the input sequence that satisfy the filter condition.</returns>
    public static IEnumerable<TSource>? FilterDynamic<TSource>(this IEnumerable<TSource> source, IDynamicFilterProvider<TSource>? filterProvider)
    {
        if (source == null)
        {
            return source;
        }

        var filter = filterProvider?.GetFilter()
            ?? (_ => true);

        var result = source?
            .Where(filter);

        return result ?? Enumerable.Empty<TSource>();
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending or descending order based on a dynamic order provider.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity to apply the dynamic ordering to.</typeparam>
    /// <param name="source">The source sequence to sort.</param>
    /// <param name="orderProvider">The dynamic order provider containing ordering criteria.</param>
    /// <returns>An IOrderedEnumerable whose elements are sorted according to the order criteria.</returns>
    public static IEnumerable<TSource>? OrderDynamic<TSource>(this IEnumerable<TSource> source, IDynamicOrderProvider<TSource>? orderProvider)
    {
        if (source == null)
        {
            return source;
        }

        var orderDelegates = orderProvider?.GetOrderDelegates()
            ?? null;

        if (orderDelegates == null || orderDelegates.Count == 0)
        {
            return source;
        }

        IOrderedEnumerable<TSource>? orderedEnumerable = null;

        foreach (var (orderDelegate, isDescending) in orderDelegates)
        {
            if (orderDelegate == null)
            {
                continue;
            }

            if (orderedEnumerable == null)
            {
                orderedEnumerable = isDescending
                    ? source.OrderByDescending(orderDelegate)
                    : source.OrderBy(orderDelegate);
            }
            else
            {
                orderedEnumerable = isDescending
                    ? orderedEnumerable.ThenByDescending(orderDelegate)
                    : orderedEnumerable.ThenBy(orderDelegate);
            }
        }

        return orderedEnumerable ?? source;
    }

    /// <summary>
    /// Returns distinct elements from a sequence based on a key selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key to distinguish elements by.</typeparam>
    /// <param name="source">The sequence to remove duplicate elements from.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An IEnumerable that contains distinct elements from the source sequence.</returns>
    /// <remarks>
    /// Uses the built-in DistinctBy method when targeting .NET 5.0 or later.
    /// Falls back to custom implementation for .NET Standard 2.0 compatibility.
    /// </remarks>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
#if NET5_0_OR_GREATER
        return source.DistinctBy(keySelector);
#else
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var seenKeys = new HashSet<TKey>();

        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
#endif
    }
}
