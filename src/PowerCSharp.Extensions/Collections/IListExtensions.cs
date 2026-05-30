using System;
using System.Collections.Generic;

namespace PowerCSharp.Extensions.Collections;

/// <summary>
/// Extension methods for IList collections
/// </summary>
public static class IListExtensions
{
    /// <summary>
    /// Removes all elements of a sequence that satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to remove elements from.</param>
    /// <param name="match">The predicate condition to match elements for removal.</param>
    /// <returns>The number of elements removed from the list.</returns>
    public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
    {
        int count = 0;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (match(list[i]))
            {
                ++count;
                list.RemoveAt(i);
            }
        }

        return count;
    }
}
