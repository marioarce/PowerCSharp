using System;
using System.IO;
using System.Security;

namespace PowerCSharp.Extensions.IO;

/// <summary>
/// Extension methods for secure path operations.
/// Provides CWE-73 compliant path combination and validation.
/// </summary>
public static class PathExtensions
{
    /// <summary>
    /// Combines path segments and validates the result to prevent directory traversal attacks (CWE-73).
    /// This method follows Veracode's recommended approach for path validation by canonicalizing
    /// the input and ensuring the result stays within the allowed base directory.
    /// </summary>
    /// <param name="basePath">The base directory path that serves as the security boundary.</param>
    /// <param name="relativePath">The relative path to combine with the base path.</param>
    /// <returns>The validated absolute path that is guaranteed to be within the base directory.</returns>
    /// <exception cref="ArgumentNullException">Thrown when basePath or relativePath is null.</exception>
    /// <exception cref="ArgumentException">Thrown when basePath or relativePath is empty.</exception>
    /// <exception cref="SecurityException">Thrown when the combined path attempts directory traversal.</exception>
    /// <remarks>
    /// This method implements the Veracode CWE-73 remediation pattern:
    /// 1. Combine the paths using Path.Combine()
    /// 2. Canonicalize using Path.GetFullPath() to resolve ".", "..", "~" directives
    /// 3. Validate the canonicalized path stays within the base directory
    /// 4. Log security events for monitoring and detection
    /// 
    /// Usage example:
    /// <code>
    /// var safePath = PathExtensions.CombineAndValidate(baseDirectory, userFileName);
    /// </code>
    /// </remarks>
    public static string CombineAndValidate(string basePath, string relativePath)
    {
        // Validate input parameters (R4: Always perform null validation)
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(relativePath);
#else
        if (basePath == null)
        {
            throw new ArgumentNullException(nameof(basePath));
        }
        if (relativePath == null)
        {
            throw new ArgumentNullException(nameof(relativePath));
        }
#endif

        if (string.IsNullOrEmpty(basePath))
        {
            throw new ArgumentException("Base path cannot be empty.", nameof(basePath));
        }

        if (string.IsNullOrEmpty(relativePath))
        {
            throw new ArgumentException("Relative path cannot be empty.", nameof(relativePath));
        }

        try
        {
            // Step 1: Combine the paths (same as original Path.Combine usage)
            var combinedPath = Path.Combine(basePath, relativePath);

            // Step 2: Canonicalize the path to resolve ".", "..", "~" and other directives
            var absolutePath = Path.GetFullPath(combinedPath);

            // Step 3: Get the absolute base path for boundary validation
            var absoluteBasePath = Path.GetFullPath(basePath);

            // Step 4: Validate the path stays within the base directory (CWE-73 protection)
            if (!absolutePath.StartsWith(absoluteBasePath, StringComparison.OrdinalIgnoreCase))
            {
                // Log security event for monitoring
                LogSecurityEvent("Path traversal attempt detected", combinedPath, absoluteBasePath);

                throw new SecurityException(
                    $"Path traversal detected. Combined path '{absolutePath}' is outside base directory '{absoluteBasePath}'. " +
                    "This attempt has been logged for security monitoring.");
            }

            // Step 5: Return the validated absolute path
            return absolutePath;
        }
        catch (SecurityException)
        {
            // Re-throw security exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            // Log unexpected failures for security monitoring
            LogSecurityEvent("Path validation failed", $"{basePath}|{relativePath}", ex.Message);

            // Wrap in SecurityException to maintain consistent error handling
            throw new SecurityException($"Path validation failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Combines multiple path segments and validates the result to prevent directory traversal attacks (CWE-73).
    /// This is an overload that accepts multiple path segments similar to Path.Combine(params string[]).
    /// </summary>
    /// <param name="basePath">The base directory path that serves as the security boundary.</param>
    /// <param name="paths">Additional path segments to combine.</param>
    /// <returns>The validated absolute path that is guaranteed to be within the base directory.</returns>
    /// <exception cref="ArgumentNullException">Thrown when basePath or paths is null.</exception>
    /// <exception cref="ArgumentException">Thrown when basePath is empty or no paths provided.</exception>
    /// <exception cref="SecurityException">Thrown when the combined path attempts directory traversal.</exception>
    /// <remarks>
    /// This method provides the same security guarantees as CombineAndValidate(string, string)
    /// but supports multiple path segments for convenience.
    /// </remarks>
    public static string CombineAndValidate(string basePath, params string[] paths)
    {
        // Validate input parameters
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(paths);
#else
        if (basePath == null)
        {
            throw new ArgumentNullException(nameof(basePath));
        }
        if (paths == null)
        {
            throw new ArgumentNullException(nameof(paths));
        }
#endif

        if (string.IsNullOrEmpty(basePath))
        {
            throw new ArgumentException("Base path cannot be empty.", nameof(basePath));
        }

        if (paths.Length == 0)
        {
            throw new ArgumentException("At least one path segment must be provided.", nameof(paths));
        }

        try
        {
            // Step 1: Combine all paths using Path.Combine with array support
            var allPaths = new string[paths.Length + 1];
            allPaths[0] = basePath;
            Array.Copy(paths, 0, allPaths, 1, paths.Length);

            var combinedPath = Path.Combine(allPaths);

            // Step 2: Canonicalize the path
            var absolutePath = Path.GetFullPath(combinedPath);

            // Step 3: Get the absolute base path for boundary validation
            var absoluteBasePath = Path.GetFullPath(basePath);

            // Step 4: Validate the path stays within the base directory
            if (!absolutePath.StartsWith(absoluteBasePath, StringComparison.OrdinalIgnoreCase))
            {
                // Log security event for monitoring
                LogSecurityEvent("Path traversal attempt detected (multiple segments)", combinedPath, absoluteBasePath);

                throw new SecurityException(
                    $"Path traversal detected. Combined path '{absolutePath}' is outside base directory '{absoluteBasePath}'. " +
                    "This attempt has been logged for security monitoring.");
            }

            // Step 5: Return the validated absolute path
            return absolutePath;
        }
        catch (SecurityException)
        {
            // Re-throw security exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            // Log unexpected failures for security monitoring
            var pathSegments = string.Join("|", paths);

            LogSecurityEvent("Path validation failed (multiple segments)", $"{basePath}|{pathSegments}", ex.Message);

            // Wrap in SecurityException to maintain consistent error handling
            throw new SecurityException($"Path validation failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Logs security events for CWE-73 violations.
    /// This provides a foundation for security monitoring infrastructure.
    /// </summary>
    /// <param name="eventType">The type of security event.</param>
    /// <param name="input">The input that triggered the event.</param>
    /// <param name="context">Additional context information.</param>
    private static void LogSecurityEvent(string eventType, string input, string context)
    {
        try
        {
            // TODO: Integrate with existing security event logging system
            // This should integrate with the existing security event logging infrastructure
            var message = $"CWE-73 Security Violation: {eventType} - Input: '{input}', Context: '{context}'";
            
            // For now, we'll use a simple approach. In production, this should integrate
            // with the existing security event logging system
            System.Diagnostics.Debug.WriteLine($"SECURITY: {message}");
            
            // TODO: Integrate with security event logging system
            // This would require access to the configured security event logger
        }
        catch
        {
            // Fail-safe: don't let logging failures break security validation
        }
    }
}
