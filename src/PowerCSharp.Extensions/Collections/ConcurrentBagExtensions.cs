using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PowerCSharp.Extensions.Collections;

/// <summary>
/// Provides extension methods for the <see cref="ConcurrentBag{T}"/> class to enhance its functionality.
/// This class contains methods that simplify common operations with concurrent bags while maintaining thread safety.
/// </summary>
public static class ConcurrentBagExtensions
{
    /// <summary>
    /// Adds a collection of elements to the concurrent bag.
    /// </summary>
    /// <typeparam name="T">The type of elements in the concurrent bag.</typeparam>
    /// <param name="this">The concurrent bag to add elements to.</param>
    /// <param name="toAdd">The collection of elements to add to the bag.</param>
    /// <remarks>
    /// This method iterates through the provided collection and adds each element individually to the concurrent bag.
    /// The operation is thread-safe as each individual Add operation on ConcurrentBag is thread-safe.
    /// </remarks>
    public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
    {
        foreach (var element in toAdd)
        {
            @this.Add(element);
        }
    }
}
