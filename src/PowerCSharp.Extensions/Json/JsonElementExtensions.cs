using System;
using System.Linq;
using System.Text.Json;

namespace PowerCSharp.Extensions.Json;

/// <summary>
/// Extension methods for JsonElement with case-insensitive property access
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
}
