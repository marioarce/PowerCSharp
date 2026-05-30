using System;

namespace PowerCSharp.Extensions.Linq.Interfaces;

/// <summary>
/// Implement a Service Provider for dynamically filtering of the type <typeparamref name="T"/> using Dynamic Expressions.
/// </summary>
/// <typeparam name="T">The type of the entity to apply the dynamic filter.</typeparam>
public interface IDynamicFilterProvider<T>
{
    /// <summary>
    /// Sets delegate to use to filter
    /// </summary>
    /// <param name="filter">A predicate that takes an object of type <typeparamref name="T"/> and returns a <see cref="bool"/>.</param>
    void SetFilter(Func<T, bool> filter);

    /// <summary>
    /// Gets the delegate to be used to filter the <typeparamref name="T"/>
    /// </summary>
    /// <returns>A predicate that takes an object of type <typeparamref name="T"/> and returns a <see cref="bool"/>.</returns>
    Func<T, bool> GetFilter();
}
