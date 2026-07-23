using PowerCSharp.Feature.Sanitization.Abstractions;

namespace WebSample.Samples.Features;

/// <summary>
/// Sample endpoint demonstrating the Sanitization feature, resolved via the Features Framework
/// (<c>PowerCSharp.Feature.Sanitization</c>).
/// </summary>
public static class SanitizationSampleEndpoints
{
    /// <summary>
    /// Gets sanitization demo data covering all four concerns: log injection (CWE-117),
    /// file-path traversal (CWE-22), sensitive-data masking (CWE-200), and regex-injection/ReDoS
    /// validation (CWE-400/CWE-730), using the DI-resolved <see cref="ISanitizationService"/>.
    /// </summary>
    /// <param name="sanitizer">The sanitization service, injected by the Features Framework.</param>
    /// <returns>Demo results for each sanitization concern.</returns>
    public static object GetDemoData(ISanitizationService sanitizer)
    {
        const string maliciousLogInput = "user logged in\r\nADMIN: fake audit entry injected";
        const string maliciousPath = "../../etc/passwd";
        const string secretPayload = "password=SuperSecret123, token=abcdef0123456789";
        const string unsafeRegexPattern = "(a+)+$"; // classic catastrophic-backtracking shape

        var logResult = sanitizer.SanitizeForLogInjection(maliciousLogInput);
        var pathResult = sanitizer.SanitizeForFilePath(maliciousPath);
        var sensitiveResult = sanitizer.SanitizeForSensitiveData(secretPayload);
        var regexResult = sanitizer.SanitizeForRegexInjection(unsafeRegexPattern);

        return new
        {
            note = "Falls back to a NoOp sanitizer (inputs pass through unchanged) if the Sanitization feature is disabled.",
            logInjection = new
            {
                original = maliciousLogInput,
                sanitized = logResult.SanitizedValue,
                wasModified = logResult.WasModified
            },
            filePathTraversal = new
            {
                original = maliciousPath,
                wasRejected = pathResult.IsRejected
            },
            sensitiveDataMasking = new
            {
                original = secretPayload,
                masked = sensitiveResult.SanitizedValue,
                wasModified = sensitiveResult.WasModified
            },
            regexInjection = new
            {
                original = unsafeRegexPattern,
                wasRejected = regexResult.IsRejected
            }
        };
    }
}
