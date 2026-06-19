using PowerCSharp.Extensions.IO;

namespace WebSample.Samples.Extensions;

/// <summary>
/// Sample endpoints demonstrating Path extensions
/// </summary>
public static class PathSampleEndpoints
{
    /// <summary>
    /// Gets Path extensions demo data
    /// </summary>
    /// <returns>Demo results showing Path operations</returns>
    public static object GetDemoData()
    {
        string basePath = "/var/www/uploads";
        string[] validPaths = { "images/photo.jpg", "documents/2023/report.pdf", "user/data/profile.json" };
        string[] maliciousPaths = { "../../etc/passwd", "../../../root/.ssh/id_rsa", "/etc/shadow", "..\\..\\windows\\system32\\config\\sam" };
        
        return new
        {
            basePath = basePath,
            validPaths = validPaths.Select(path => new
            {
                input = path,
                result = TryCombineAndValidate(basePath, path),
                success = true
            }),
            maliciousPaths = maliciousPaths.Select(path => new
            {
                input = path,
                result = TryCombineAndValidate(basePath, path),
                success = false
            }),
            multiSegmentDemo = TryCombineAndValidate(basePath, "documents", "2023", "reports", "annual.pdf"),
            securityFeatures = new
            {
                description = "CWE-73 compliant path operations with directory traversal protection",
                securityStandard = "Veracode CWE-73 remediation pattern",
                protectionMechanisms = new[]
                {
                    "Path canonicalization using Path.GetFullPath()",
                    "Validation that combined path stays within base directory",
                    "Security event logging for monitoring",
                    "Consistent error handling with SecurityException"
                },
                attackVectorsPrevented = new[]
                {
                    "../ directory traversal",
                    "Absolute path injection",
                    "Windows-style backslash traversal",
                    "Encoded path traversal attempts"
                }
            },
            usageExamples = new
            {
                fileUploadSystem = "Secure user file uploads with validation",
                documentManagement = "Safe document path construction",
                assetServing = "Protected static asset access",
                configurationFiles = "Secure configuration file paths"
            },
            performance = new
            {
                optimized = "Minimal allocation path operations",
                logging = "Security event logging without performance impact",
                validation = "Fast path validation with early exit on violations"
            }
        };
    }

    /// <summary>
    /// Helper method to safely test CombineAndValidate and return appropriate result
    /// </summary>
    private static object TryCombineAndValidate(string basePath, string relativePath)
    {
        try
        {
            var result = PathExtensions.CombineAndValidate(basePath, relativePath);
            return new { success = true, path = result, error = (string?)null };
        }
        catch (Exception ex)
        {
            return new { success = false, path = (string?)null, error = ex.Message };
        }
    }

    /// <summary>
    /// Helper method to safely test CombineAndValidate with multiple segments
    /// </summary>
    private static object TryCombineAndValidate(string basePath, params string[] paths)
    {
        try
        {
            var result = PathExtensions.CombineAndValidate(basePath, paths);
            return new { success = true, path = result, error = (string?)null };
        }
        catch (Exception ex)
        {
            return new { success = false, path = (string?)null, error = ex.Message };
        }
    }
}
