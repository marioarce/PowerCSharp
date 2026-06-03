using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PowerCSharp.Extensions.Types;

/// <summary>
/// Extension methods for generic type operations
/// </summary>
public static class GenericTypeExtensions
{
    /// <summary>
    /// Check if the value is equal to its default value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value equals its default; otherwise, false.</returns>
    public static bool IsDefault<T>(this T value) =>
        Equals(value, default(T));

    /// <summary>
    /// Returns the name of the generic type of the object, including generic parameters.
    /// </summary>
    /// <param name="object">The object to get the generic type name from.</param>
    /// <returns>The name of the generic type with parameters, or the simple type name if not generic.</returns>
    public static string GetGenericTypeName(this object @object)
    {
        if (@object == null)
        {
            return string.Empty;
        }

        var type = @object.GetType();

        // Check if the type is not generic
        if (!type.IsGenericType)
            return type.Name;

        // Get the names of the generic arguments and join them with commas
        var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());

        // Remove the backtick and append the generic arguments to the type name
        return $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
    }

    /// <summary>
    /// Traverses a hierarchical structure starting from the source element.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the hierarchy.</typeparam>
    /// <param name="source">The starting element in the hierarchy.</param>
    /// <param name="nextItem">Function to get the next element in the hierarchy.</param>
    /// <param name="canContinue">Function to determine if traversal should continue for the current element.</param>
    /// <returns>An enumerable that yields each element in the hierarchy in traversal order.</returns>
    /// <example>
    /// <code>
    /// // Traverse a tree structure from root to leaves
    /// var nodes = rootNode.FromHierarchy(node => node.Parent, node => node != null);
    /// foreach (var node in nodes)
    /// {
    ///     Console.WriteLine(node.Name);
    /// }
    /// </code>
    /// </example>
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
    /// Traverses a hierarchical structure starting from the source element, continuing until null is encountered.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the hierarchy. Must be a reference type.</typeparam>
    /// <param name="source">The starting element in the hierarchy.</param>
    /// <param name="nextItem">Function to get the next element in the hierarchy.</param>
    /// <returns>An enumerable that yields each element in the hierarchy in traversal order.</returns>
    /// <remarks>
    /// This is a convenience method that uses a null check as the continuation condition.
    /// Equivalent to: FromHierarchy(source, nextItem, s => s != null)
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find all parent objects up the chain
    /// var parents = childObject.FromHierarchy(obj => obj.Parent);
    /// </code>
    /// </example>
    public static IEnumerable<TSource> FromHierarchy<TSource>(
        this TSource source,
        Func<TSource, TSource> nextItem)
        where TSource : class
    {
        return FromHierarchy(source, nextItem, s => s != null);
    }

    /// <summary>
    /// Copies matching properties from the source object to the destination object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object. Must be a reference type.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object. Must be a reference type.</typeparam>
    /// <param name="source">The object to copy properties from.</param>
    /// <param name="destination">The object to copy properties to.</param>
    /// <remarks>
    /// Properties are copied based on both name and type compatibility:
    /// - Property names must match exactly (case-sensitive)
    /// - Destination property type must be assignable from source property type
    /// - Only readable source properties and writable destination properties are considered
    /// - If multiple matching source properties exist, all will be applied (though typically there should be only one)
    /// </remarks>
    /// <example>
    /// <code>
    /// // Copy data between similar objects
    /// var userDto = new UserDto { Name = "John", Age = 30 };
    /// var userEntity = new UserEntity();
    /// userDto.CopyPropertiesTo(userEntity);
    /// // userEntity.Name = "John", userEntity.Age = 30
    /// </code>
    /// </example>
    public static void CopyPropertiesTo<TSource, TDestination>(this TSource source, TDestination destination)
        where TSource : class
        where TDestination : class
    {
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
                // should only be one
                destprop.SetValue(destination, sourceprop.GetValue(source, null), null);
            }
        }
    }
}
