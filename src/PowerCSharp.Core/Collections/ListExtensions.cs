using System;
using System.Collections.Generic;

namespace PowerCSharp.Core.Collections;

/// <summary>
/// Extension methods for List operations.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Creates a deep clone of a list where each element implements ICloneable.
    /// Each element in the list is cloned using its Clone() method.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list, must implement ICloneable.</typeparam>
    /// <param name="sourceList">The source list to clone.</param>
    /// <returns>A new list containing deep-cloned elements, or null if the input is null or elements are not cloneable.</returns>
    /// <remarks>
    /// This method requires that all elements in the list implement the ICloneable interface.
    /// If any element does not implement ICloneable, the method returns null.
    /// </remarks>
    public static List<T>? DeepClone<T>(this List<T>? sourceList)
        where T : ICloneable
    {
        if (sourceList == null)
        {
            return null;
        }

        // Check if the type T implements ICloneable
        bool isCloneable = typeof(ICloneable).IsAssignableFrom(typeof(T));
        if (!isCloneable)
        {
            return null;
        }

        // Perform a deep copy/clone of each element
        var clonedList = new List<T>(sourceList.Count);
        foreach (var item in sourceList)
        {
            clonedList.Add(item != null ? (T)item.Clone() : default);
        }

        return clonedList;
    }

    /// <summary>
    /// Returns a new list containing only the first occurrence of each unique item based on the specified key selector.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <typeparam name="TKey">The type of the key to compare for uniqueness.</typeparam>
    /// <param name="sourceList">The source list to deduplicate.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>A new list with duplicates removed based on the specified key, or null if the input is null.</returns>
    public static List<T>? DistinctBy<T, TKey>(this List<T>? sourceList, Func<T, TKey> keySelector)
    {
        if (sourceList == null)
        {
            return null;
        }

        var seenKeys = new HashSet<TKey>();
        var result = new List<T>();

        foreach (var item in sourceList)
        {
            var key = keySelector(item);
            if (seenKeys.Add(key))
            {
                result.Add(item);
            }
        }

        return result;
    }
}
