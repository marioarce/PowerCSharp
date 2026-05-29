using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PowerCSharp.Extensions.Objects;

/// <summary>
/// Extension methods for generic operations and object manipulation
/// </summary>
public static class GenericExtensions
{
    /// <summary>
    /// Processes a structure hierarchically using the provided next item function and continuation condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object to start processing from.</param>
    /// <param name="nextItem">Function to get the next item in the hierarchy.</param>
    /// <param name="canContinue">Function to determine if processing should continue.</param>
    /// <returns>An enumerable of items in the hierarchy.</returns>
    public static IEnumerable<TSource> FromHierarchy<TSource>(
        this TSource source,
        Func<TSource, TSource> nextItem,
        Func<TSource, bool> canContinue)
    {
        for (var current = source; canContinue(current); current = nextItem(current))
        {
            yield return current;
        }
    }

    /// <summary>
    /// Processes a structure hierarchically until null is encountered.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object to start processing from.</param>
    /// <param name="nextItem">Function to get the next item in the hierarchy.</param>
    /// <returns>An enumerable of items in the hierarchy.</returns>
    public static IEnumerable<TSource> FromHierarchy<TSource>(
        this TSource source,
        Func<TSource, TSource> nextItem)
        where TSource : class
    {
        return FromHierarchy(source, nextItem, s => s != null);
    }

    /// <summary>
    /// Copies all matching properties by name and type from the source object to the destination object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <param name="source">The source object to copy properties from.</param>
    /// <param name="destination">The destination object to copy properties to.</param>
    public static void CopyPropertiesTo<TSource, TDestination>(this TSource source, TDestination destination)
        where TSource : class
        where TDestination : class
    {
        if (source == null || destination == null)
        {
            return;
        }

        var sourceProperties = from prop1 in typeof(TSource).GetProperties()
                               where prop1.CanRead
                               select prop1;
        var destinationProperties = from prop2 in typeof(TDestination).GetProperties()
                                    where prop2.CanWrite
                                    select prop2;

        foreach (PropertyInfo destprop in destinationProperties)
        {
            var sourceprops = sourceProperties
                .Where((p) => p.Name == destprop.Name && destprop.PropertyType.IsAssignableFrom(p.PropertyType));

            foreach (PropertyInfo sourceprop in sourceprops)
            {
                // Should only be one matching property
                destprop.SetValue(destination, sourceprop.GetValue(source, null), null);
            }
        }
    }
}
