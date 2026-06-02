using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PowerCSharp.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Adds or updates the specified key and value to the dictionary
    /// </summary>
    /// <param name="key">The key of the element to add or update</param>
    /// <param name="value">The value of the element to add or update. The value can be null for reference types</param>
    /// <exception cref="ArgumentNullException">The dictionary is null</exception>
    /// <exception cref="ArgumentNullException">The key is null</exception>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }

    /// <summary>
    /// Adds the specified key and value to the dictionary
    /// </summary>
    /// <param name="key">The key of the element to add</param>
    /// <param name="value">The value of the element to add. The value can be null for reference types</param>
    /// <returns><code>true</code> if the entry was added</returns>
    /// <exception cref="ArgumentNullException">The dictionary is null</exception>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or returns a default value if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to retrieve the value from.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="defaultValue">The default value to return if the key does not exist. Defaults to the default value of TValue.</param>
    /// <returns>The value associated with the specified key if found; otherwise, the default value.</returns>
    /// <exception cref="ArgumentNullException">The dictionary is null</exception>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// A NameValueCollection extension method that converts the @this to a dictionary.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as an IDictionary&lt;string,object&gt;</returns>
    public static IDictionary<string, string> ToDictionary(this NameValueCollection @this)
    {
        var result = new Dictionary<string, string>();

        if (@this != null)
        {
            foreach (string key in @this.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                var value = @this[key];
                if (key != null && value != null)
                {
                    result.AddOrUpdate(key, value);
                }
            }
        }

        return result;
    }
}
