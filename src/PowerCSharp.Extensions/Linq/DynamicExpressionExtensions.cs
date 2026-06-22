using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using PowerCSharp.Core.Interfaces.Extensions.Linq;

namespace PowerCSharp.Extensions.Linq;

/// <summary>
/// Extension methods for dynamic LINQ expression parsing and evaluation
/// </summary>
public static class DynamicExpressionExtensions
{
    /// <summary>
    /// Parses a LINQ expression string into a compiled predicate function.
    /// </summary>
    /// <typeparam name="T">The type of the entity to apply the predicate to.</typeparam>
    /// <param name="stringExpression">The string expression to parse.</param>
    /// <returns>A predicate function that takes an object of type <typeparamref name="T"/> and returns a <see cref="bool"/>.</returns>
    /// <remarks>If parsing fails, returns a function that always returns true to avoid affecting the entire operation.</remarks>
    public static Func<T, bool> GetExpressionDelegate<T>(this string stringExpression)
    {
        try
        {
            var lambdaExpression = DynamicExpressionParser.ParseLambda(
                typeof(T),
                typeof(bool),
                stringExpression
            );

            var expression = (Expression<Func<T, bool>>)
                lambdaExpression;

            var result = expression
                .Compile();

            return result;
        }
        catch
        {
            // Return default value to not affect the entire operation due to parsing error
            static bool result(T _) => true;
            return result;
        }
    }

    /// <summary>
    /// Parses a LINQ expression string into a list of ordering delegates.
    /// </summary>
    /// <typeparam name="TSource">The type of the entity to apply the ordering to.</typeparam>
    /// <param name="stringExpression">The string expression containing ordering clauses.</param>
    /// <returns>A list of tuples containing key selector functions and descending flags for ordering.</returns>
    public static List<(Func<TSource, object>, bool)> GetOrderDelegates<TSource>(this string stringExpression)
    {
        var result = new List<(Func<TSource, object>, bool)>();

        if (string.IsNullOrEmpty(stringExpression))
        {
            return result;
        }

        var orderClauses = new List<OrderClause>();
        var clauses = stringExpression.Trim().Split(',');
        foreach (var clause in clauses)
        {
            var parts = clause.Trim().Split(' ');
            orderClauses.Add(new OrderClause
            {
                Property = parts[0],
                Descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase)
            });
        }

        if (orderClauses.Count == 0)
        {
            return result;
        }

        foreach (var orderClause in orderClauses)
        {
            var propertyName = orderClause.Property;
            var isDescending = orderClause.Descending;
            
            if (string.IsNullOrEmpty(propertyName?.Trim()))
            {
                continue;
            }

            try
            {
                var keySelector = CreateKeySelector<TSource>(propertyName);

                result.Add(
                    (keySelector, isDescending)
                );
            }
            catch
            {
                // Do nothing to avoid affecting the entire order expression due to error in a particular part
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a function to extract a key from an element using the specified property name.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="propertyName">The name of the property to extract.</param>
    /// <returns>A function that extracts the specified property value from an object.</returns>
    private static Func<TSource, object> CreateKeySelector<TSource>(string propertyName)
    {
        var param = Expression.Parameter(typeof(TSource), "x");
        var property = Expression.Property(param, propertyName);
        var converted = Expression.Convert(property, typeof(object));

        var lambda = Expression.Lambda<Func<TSource, object>>(converted, param);
        var result = lambda.Compile();

        return result;
    }
}
