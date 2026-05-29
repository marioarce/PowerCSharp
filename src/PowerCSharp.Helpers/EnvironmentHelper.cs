using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PowerCSharp.Helpers;

/// <summary>
/// Helper class for environment and system operations
/// </summary>
public static partial class EnvironmentHelper
{
#if NET8_0_OR_GREATER
    private static readonly Regex SafeMachineNameRegex = SafeMachineNameRegexMethod();
#else
    private static readonly Regex SafeMachineNameRegex = new(@"[^a-zA-Z0-9\-_]", RegexOptions.Compiled);
#endif

    /// <summary>
    /// Gets environment variable safely with default value
    /// </summary>
    public static string GetEnvironmentVariable(string name, string defaultValue = "")
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue;
    }

    /// <summary>
    /// Checks if running in development environment
    /// </summary>
    public static bool IsDevelopment()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
               ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        return env?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Gets machine name in safe format
    /// </summary>
    public static string GetSafeMachineName()
    {
        var name = Environment.MachineName;
        return SafeMachineNameRegex.Replace(name, "_");
    }

    /// <summary>
    /// Gets current application version
    /// </summary>
    public static string GetApplicationVersion()
    {
        return Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "1.0.0.0";
    }

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"[^a-zA-Z0-9\-_]", RegexOptions.Compiled)]
    private static partial Regex SafeMachineNameRegexMethod();
#endif
}
