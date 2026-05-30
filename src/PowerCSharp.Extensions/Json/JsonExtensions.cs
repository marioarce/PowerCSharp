using System;
using System.Text.Json;

namespace PowerCSharp.Extensions.Json;

/// <summary>
/// Extension methods for JSON element operations
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Looks for a property with the specified name in the current JSON element.
    /// </summary>
    /// <param name="element">The JSON element to search within.</param>
    /// <param name="name">The name of the property to find.</param>
    /// <returns>The JsonElement value if found and valid; otherwise, null.</returns>
    public static JsonElement? Get(this JsonElement element, string name)
    {
        var result = (element.ValueKind != JsonValueKind.Null) && 
                     element.ValueKind != JsonValueKind.Undefined && 
                     element.TryGetProperty(name, out var value)
            ? value
            : (JsonElement?)null;

        return result;
    }

    /// <summary>
    /// Looks for a property by index in the current JSON array element.
    /// </summary>
    /// <param name="element">The JSON element to search within.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>The JsonElement at the specified index if found; otherwise, null.</returns>
    public static JsonElement? Get(this JsonElement element, int index)
    {
        if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        // Return null if index is out of range
        return index < element.GetArrayLength()
            ? element[index]
            : null;
    }
}
