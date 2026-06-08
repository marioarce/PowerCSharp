using System;
using System.Linq;
using System.Text.Json;

namespace PowerCSharp.Extensions.Json;

/// <summary>
/// Extension methods for JsonElement with case-insensitive property access and convenient getters
/// </summary>
public static class JsonElementExtensions
{
    /// <summary>
    /// Looks for a property with the specified name using case-insensitive comparison.
    /// </summary>
    /// <param name="element">The JSON element to search within.</param>
    /// <param name="propertyName">The name of the property to find (case-insensitive).</param>
    /// <param name="value">When this method returns, contains the JsonElement value if the property was found; otherwise, the default value.</param>
    /// <returns>True if the property exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when propertyName is null.</exception>
    public static bool TryGetPropertyCaseInsensitive(this JsonElement element, string propertyName, out JsonElement value)
    {
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));

        var result = false;
        value = default;

        var property = element
            .EnumerateObject()
            .FirstOrDefault(p => string.Compare(p.Name, propertyName, StringComparison.OrdinalIgnoreCase) == 0);

        if (property.Value.ValueKind != JsonValueKind.Undefined)
        {
            value = property.Value;
            result = true;
        }

        return result;
    }

    /// <summary>
    /// Looks for a property named <paramref name="name"/> in the current JSON object.
    /// Returns null if the element is null, undefined, or the property doesn't exist.
    /// </summary>
    /// <param name="element">The JSON element to search within.</param>
    /// <param name="name">The name of the property to find (case-sensitive).</param>
    /// <returns>The JsonElement value if found; otherwise, null.</returns>
    public static JsonElement? Get(this JsonElement element, string name)
    {
        var result = (element.ValueKind != JsonValueKind.Null) && element.ValueKind != JsonValueKind.Undefined && element.TryGetProperty(name, out var value)
            ? value
            : (JsonElement?)null;

        return result;
    }

    /// <summary>
    /// Looks for a property by index in the current JSON array.
    /// Returns null if the element is null, undefined, not an array, or the index is out of bounds.
    /// </summary>
    /// <param name="element">The JSON element to search within.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>The JsonElement value if found; otherwise, null.</returns>
    public static JsonElement? Get(this JsonElement element, int index)
    {
        if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        // Return null if index is out of bounds
        return index < element.GetArrayLength()
            ? element[index]
            : null;
    }
}
