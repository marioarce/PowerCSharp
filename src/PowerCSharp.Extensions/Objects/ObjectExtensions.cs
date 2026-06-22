using System;
using System.Reflection;

namespace PowerCSharp.Extensions.Objects;

/// <summary>
/// Extension methods for general object operations
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check for null.</param>
    /// <returns>The original value if not null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public static T ThrowOnNull<T>(this T? value)
        where T : class => value ?? throw new ArgumentNullException();

    /// <summary>
    /// Attempts to convert various types of input into a boolean value.
    /// Supports <see cref="bool"/>, numeric types, and strings representing boolean or numeric values.
    /// </summary>
    /// <param name="value">The input value to convert to boolean.</param>
    /// <param name="result">When this method returns, contains the boolean equivalent of the input value, if the conversion succeeded, or <c>false</c> if the conversion failed.</param>
    /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// Examples:
    ///   TryGetBool(true, out var b1);     // true
    ///   TryGetBool("true", out var b2);   // true
    ///   TryGetBool("false", out var b3);  // false
    ///   TryGetBool("1", out var b4);      // true
    ///   TryGetBool("0", out var b5);      // false
    ///   TryGetBool("2", out var b6);      // true (since != 0)
    ///   TryGetBool(5, out var b7);        // true
    ///   TryGetBool(0, out var b8);        // false
    ///   TryGetBool("abc", out var b9);    // false (parse fails)
    /// </code>
    /// </example>
    public static bool TryGetBool(this object value, out bool result)
    {
        result = false;

        if (value == null)
        {
            return false;
        }

        switch (value)
        {
            case bool b:
                result = b;
                return true;

            case string s:
                // First try normal bool parsing ("true"/"false")
                if (bool.TryParse(s, out var parsed))
                {
                    result = parsed;
                    return true;
                }

                // Then try numeric string ("0"/"1"/"2"/etc.)
                if (int.TryParse(s, out var number))
                {
                    result = number != 0;
                    return true;
                }
                return false;

            case IConvertible convertible:
                try
                {
                    // For numbers (int, long, byte, etc.)
                    if (convertible is IFormattable)
                    {
                        var integer = convertible.ToInt32(System.Globalization.CultureInfo.InvariantCulture);
                        result = integer != 0;
                        return true;
                    }

                    result = Convert.ToBoolean(convertible);
                    return true;
                }
                catch
                {
                    return false;
                }
        }

        return false;
    }

    /// <summary>
    /// Maps properties from the source object to a new destination object of type TDATA.
    /// </summary>
    /// <typeparam name="TDATA">The type of the destination object.</typeparam>
    /// <param name="oldObject">The source object to map properties from.</param>
    /// <returns>A new instance of TDATA with matching properties copied from the source object.</returns>
    public static TDATA? Map<TDATA>(this object oldObject)
        where TDATA : class, new()
    {
        // Create a new object of type TDATA
        TDATA newObject = new();

        try
        {
            // If the old object is null, just return the new object
            if (oldObject == null)
            {
                return newObject;
            }

            // Get the type of the new object and the type of the old object passed in
            Type newObjType = typeof(TDATA);
            Type oldObjType = oldObject.GetType();

            // Get a list of all the properties in the new object
            var propertyList = newObjType.GetProperties();
            
            // If the new object has properties
            if (propertyList.Length > 0)
            {
                // Loop through each property in the new object
                foreach (var newObjProp in propertyList)
                {
                    // Get the corresponding property in the old object
                    var oldProp = oldObjType.GetProperty(newObjProp.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.ExactBinding);

                    // If there is a corresponding property in the old object and it can be read and the new object's property can be written to
                    if (oldProp != null && oldProp.CanRead && newObjProp.CanWrite)
                    {
                        // Assign property type of both object to new variables
                        var oldPropertyType = oldProp.PropertyType;
                        var newPropertyType = newObjProp.PropertyType;

                        // Check if property is nullable or not. If property is nullable then get its original data type from generic argument
                        if (oldPropertyType.IsGenericType && oldPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            oldPropertyType = oldPropertyType.GetGenericArguments()[0];
                        if (newPropertyType.IsGenericType && newPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            newPropertyType = newPropertyType.GetGenericArguments()[0];

                        // Check type of both property if match then set value
                        if (newPropertyType == oldPropertyType)
                        {
                            // Get the value of the property in the old object
                            var value = oldProp.GetValue(oldObject);
                            // Set the value of the property in the new object
                            newObjProp.SetValue(newObject, value);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        // Return the new object
        return newObject;
    }
}
