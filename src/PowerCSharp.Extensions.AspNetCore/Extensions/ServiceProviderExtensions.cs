using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PowerCSharp.Core.Interfaces.Extensions.Configuration;

namespace PowerCSharp.Extensions.AspNetCore.Extensions;

/// <summary>
/// Extension methods for IServiceProvider operations
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Get options from the service provider.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The options.</returns>
    public static TOptions? GetOptions<TOptions>(this IServiceProvider serviceProvider)
        where TOptions : class, IAppOptions =>
        serviceProvider.GetService<IOptions<TOptions>>()?.Value;
}
