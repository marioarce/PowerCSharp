using System;
using System.Linq;

namespace PowerCSharp.Extensions.Types;

/// <summary>
/// Extension methods for Type reflection operations
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns the concrete <see cref="Type"/> that implements the specified interface type.
    /// </summary>
    /// <param name="interfaceType">The interface type to find implementations for.</param>
    /// <returns>The first concrete type found that implements the interface, or null if none found.</returns>
    public static Type? GetConcreteType(this Type interfaceType)
    {
        if (interfaceType == null || !interfaceType.IsInterface)
        {
            return null;
        }

        try
        {
            // Find all non-abstract, non-interface types that implement the given interface
            var concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ToList();

            return concreteTypes.FirstOrDefault(); // Choose the first available implementation
        }
        catch
        {
            // Return null if there's an error accessing assemblies or types
            return null;
        }
    }

    /// <summary>
    /// Determines if a type is a simple/primitive type that can be safely stored in Exception.Data or serialized efficiently.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is simple; False if it's complex.</returns>
    public static bool IsSimpleType(this Type type)
    {
        if (type == null)
        {
            return false;
        }

        var result = type.IsPrimitive || 
               type.IsEnum || 
               type == typeof(string) || 
               type == typeof(decimal) || 
               type == typeof(DateTime) || 
               type == typeof(DateTimeOffset) || 
               type == typeof(TimeSpan) || 
               type == typeof(Guid) ||
               Nullable.GetUnderlyingType(type) != null && IsSimpleType(Nullable.GetUnderlyingType(type));

        return result;
    }

    /// <summary>
    /// Checks if the source type is the same as, inherits from, or implements the target type.
    /// </summary>
    /// <param name="sourceType">The type to test.</param>
    /// <param name="targetType">The base type or interface to compare against.</param>
    /// <returns>True if the source is the same, a subclass, or implements the interface.</returns>
    public static bool IsOrInheritsFrom(this Type sourceType, Type targetType)
    {
        if (sourceType == null || targetType == null)
        {
            return false;
        }

        return targetType.IsAssignableFrom(sourceType);
    }
}
