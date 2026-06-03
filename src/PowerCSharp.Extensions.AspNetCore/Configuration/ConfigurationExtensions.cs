using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Core.Interfaces.Extensions.Configuration;

namespace PowerCSharp.Extensions.AspNetCore.Configuration;

/// <summary>
/// Extension methods for IConfiguration operations
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets options from IConfiguration object using the specified configuration section path.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to retrieve. Must implement IAppOptions.</typeparam>
    /// <param name="configuration">The IConfiguration object to read from.</param>
    /// <param name="sectionPath">The configuration section path to use.</param>
    /// <returns>The configured options object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public static TOptions GetOptions<TOptions>(this IConfiguration configuration, string sectionPath)
        where TOptions : class, IAppOptions
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        if (string.IsNullOrWhiteSpace(sectionPath))
            throw new ArgumentException("Section path cannot be null or whitespace.", nameof(sectionPath));

        var section = configuration.GetSection(sectionPath);
        var options = section.Get<TOptions>();
        
        return options ?? throw new InvalidOperationException($"Failed to bind configuration section '{sectionPath}' to type {typeof(TOptions).Name}");
    }

    /// <summary>
    /// Get options from IConfiguration object using the ConfigSectionPath from the options type.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to retrieve. Must implement IAppOptions.</typeparam>
    /// <param name="configuration">The IConfiguration object.</param>
    /// <returns>The options object.</returns>
    public static TOptions GetOptions<TOptions>(this IConfiguration configuration)
        where TOptions : class, IAppOptions
    {
        return configuration
            .GetRequiredSection(typeof(TOptions).Name)
            .Get<TOptions>(options => options.BindNonPublicProperties = true);
    }
}
