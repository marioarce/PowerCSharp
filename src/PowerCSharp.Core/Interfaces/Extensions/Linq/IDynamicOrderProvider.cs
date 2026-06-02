using System;
using System.Collections.Generic;

namespace PowerCSharp.Core.Interfaces.Extensions.Linq;

/// <summary>
/// Implement a Service Provider for dynamically ordering of the type <typeparamref name="T"/> using Dynamic Expressions.
/// </summary>
/// <typeparam name="T">The type of the entity to apply the dynamic order.</typeparam>
public interface IDynamicOrderProvider<T>
{
    /// <summary>
    /// Sets delegate to use for ordering
    /// </summary>
    /// <param name="delegates">A list of predicates that takes an object of type <typeparamref name="T"/> and returns a <see cref="object"/>.</param>
    void SetOrderDelegates(List<(Func<T, object>, bool)> delegates);

    /// <summary>
    /// Gets the list of delegates to be used to order the <typeparamref name="T"/>
    /// </summary>
    /// <returns>A list of predicates that takes an object of type <typeparamref name="T"/> and returns a <see cref="object"/>.</returns>
    List<(Func<T, object>, bool)> GetOrderDelegates();
}
