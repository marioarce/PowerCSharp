using System;
using System.Linq;

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
}
