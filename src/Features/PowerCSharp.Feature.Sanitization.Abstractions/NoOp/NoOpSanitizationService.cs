using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions.NoOp;

/// <summary>
/// Inert <see cref="ISanitizationService"/> used when the Sanitization feature is disabled.
/// Every method returns the input unchanged, so dependents resolve safely — sanitization is
/// simply inactive rather than the container failing to resolve <see cref="ISanitizationService"/>.
/// </summary>
public sealed class NoOpSanitizationService : ISanitizationService
{
    private static readonly SanitizationSettings _disabledSettings = new()
    {
        EnableLogSanitization = false,
        EnableFilePathSanitization = false,
        EnableSensitiveDataDetection = false,
        EnableRegexSanitization = false
    };

    /// <summary>Creates the NoOp sanitization service and logs that sanitization is inert.</summary>
    public NoOpSanitizationService(ILogger<NoOpSanitizationService> logger)
    {
        logger.LogInformation("Sanitization feature is disabled; using NoOp sanitization service (inputs pass through unchanged).");
    }

    /// <inheritdoc />
    public SanitizationResult SanitizeForLogInjection(string? input)
        => SanitizationResult.Unchanged(input ?? string.Empty, SanitizationType.LogInjection);

    /// <inheritdoc />
    public SanitizationResult SanitizeForLogInjection(string? input, SanitizationSettings? settings)
        => SanitizationResult.Unchanged(input ?? string.Empty, SanitizationType.LogInjection);

    /// <inheritdoc />
    public SanitizationResult SanitizeForFilePath(string? input)
        => SanitizationResult.Unchanged(input ?? string.Empty, SanitizationType.FilePath);

    /// <inheritdoc />
    public SanitizationResult SanitizeForFilePath(string? input, SanitizationSettings? settings)
        => SanitizationResult.Unchanged(input ?? string.Empty, SanitizationType.FilePath);

    /// <inheritdoc />
    public SensitiveDataResult SanitizeForSensitiveData(string? input)
        => SensitiveDataResult.Unchanged(input ?? string.Empty);

    /// <inheritdoc />
    public SensitiveDataResult SanitizeForSensitiveData(string? input, SanitizationSettings? settings)
        => SensitiveDataResult.Unchanged(input ?? string.Empty);

    /// <inheritdoc />
    public SanitizationResult SanitizeForRegexInjection(string? pattern)
        => SanitizationResult.Unchanged(pattern ?? string.Empty, SanitizationType.RegexInjection);

    /// <inheritdoc />
    public SanitizationResult SanitizeForRegexInjection(string? pattern, SanitizationSettings? settings)
        => SanitizationResult.Unchanged(pattern ?? string.Empty, SanitizationType.RegexInjection);

    /// <inheritdoc />
    public SanitizationSettings GetCurrentSettings() => _disabledSettings;
}
