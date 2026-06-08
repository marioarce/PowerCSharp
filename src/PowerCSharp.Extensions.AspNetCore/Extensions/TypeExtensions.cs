using System;

namespace PowerCSharp.Extensions.AspNetCore.Extensions;

/// <summary>
/// Extension methods for Type reflection operations
/// </summary>
public static class TypeExtensions
{
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
}
