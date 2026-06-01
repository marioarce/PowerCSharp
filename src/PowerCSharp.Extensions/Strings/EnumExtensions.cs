using System;
using System.ComponentModel;

namespace PowerCSharp.Extensions.Strings;

/// <summary>
/// Provides extension methods for working with enumerations.
/// Includes utilities for retrieving descriptions of enum values.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Retrieves the description associated with an enumeration value.
    /// The description is defined using the <see cref="DescriptionAttribute"/>.
    /// </summary>
    /// <param name="enumeration">The enumeration value to retrieve the description for.</param>
    /// <returns>
    /// A string containing the description of the enumeration value, or an empty string
    /// if no <see cref="DescriptionAttribute"/> is defined for the value.
    /// </returns>
    public static string ToDescription(this Enum enumeration)
    {
        var name = enumeration.ToString();
        Type type = enumeration.GetType();
        DescriptionAttribute[] array = type
            .GetField(name)
            .GetCustomAttributes(typeof(DescriptionAttribute), inherit: false) as DescriptionAttribute[];

        if (array == null || array.Length == 0)
        {
            return string.Empty;
        }

        return array[0].Description;
    }
}
