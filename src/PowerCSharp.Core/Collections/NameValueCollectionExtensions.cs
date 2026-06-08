using System.Collections.Generic;
using System.Collections.Specialized;

namespace PowerCSharp.Core.Collections;

/// <summary>
/// Extension methods for NameValueCollection operations.
/// </summary>
public static class NameValueCollectionExtensions
{
    /// <summary>
    /// Converts a NameValueCollection to a dictionary.
    /// Empty keys are ignored, and null values are skipped.
    /// </summary>
    /// <param name="collection">The NameValueCollection to convert.</param>
    /// <returns>A dictionary representation of the NameValueCollection, or an empty dictionary if the input is null.</returns>
    public static IDictionary<string, string> ToDictionary(this NameValueCollection collection)
    {
        var result = new Dictionary<string, string>();

        if (collection != null)
        {
            foreach (string key in collection.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                var value = collection[key];

                if (key != null && value != null)
                {
                    result.AddOrUpdate(key, value);
                }
            }
        }

        return result;
    }
}
