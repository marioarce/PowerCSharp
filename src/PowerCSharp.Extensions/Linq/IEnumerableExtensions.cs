using System;
using System.Collections.Generic;
using System.Linq;
using PowerCSharp.Extensions.Linq.Interfaces;

namespace PowerCSharp.Extensions.Linq;

/// <summary>
/// Extension methods for IEnumerable collections with dynamic filtering and ordering capabilities
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Filters a sequence of values based on a dynamic filter provider.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity to apply the dynamic filter to.</typeparam>
    /// <param name="source">The source sequence to filter.</param>
    /// <param name="filterProvider">The dynamic filter provider containing filter criteria.</param>
    /// <returns>An IEnumerable of <typeparamref name="TSource"/> that contains elements from the input sequence that satisfy the filter condition.</returns>
    public static IEnumerable<TSource> Filter<TSource>(this IEnumerable<TSource> source, IDynamicFilterProvider<TSource>? filterProvider)
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
    public static IEnumerable<TSource> Order<TSource>(this IEnumerable<TSource> source, IDynamicOrderProvider<TSource>? orderProvider)
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
}
