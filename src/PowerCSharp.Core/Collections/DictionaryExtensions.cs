using System;
using System.Collections.Generic;

namespace PowerCSharp.Core.Collections;

/// <summary>
/// Extension methods for dictionary and collection operations.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Adds or updates the specified key and value to the dictionary.
    /// If the key already exists, its value will be updated. If the key does not exist, a new key-value pair will be added.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to modify.</param>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="value">The value of the element to add or update. The value can be null for reference types.</param>
    /// <exception cref="ArgumentNullException">The dictionary is null.</exception>
    /// <exception cref="ArgumentNullException">The key is null.</exception>
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
    /// Attempts to add the specified key and value to the dictionary.
    /// If the key already exists, the method returns false and does not modify the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to modify.</param>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
    /// <returns>true if the entry was added successfully; false if the key already exists.</returns>
    /// <exception cref="ArgumentNullException">The dictionary is null.</exception>
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
}
