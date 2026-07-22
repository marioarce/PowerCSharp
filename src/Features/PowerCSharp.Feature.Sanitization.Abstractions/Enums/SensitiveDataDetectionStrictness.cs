namespace PowerCSharp.Feature.Sanitization.Abstractions.Enums;

/// <summary>
/// Defines the strictness levels for sensitive data detection.
/// Higher levels result in more aggressive detection patterns.
/// </summary>
public enum SensitiveDataDetectionStrictness
{
    /// <summary>
    /// Conservative detection - only obvious sensitive patterns are detected.
    /// Minimal false positives, but may miss some sensitive data.
    /// </summary>
    Low,

    /// <summary>
    /// Balanced detection - common sensitive patterns are detected.
    /// Good balance between false positives and false negatives.
    /// </summary>
    Medium,

    /// <summary>
    /// Aggressive detection - most potential sensitive patterns are detected.
    /// Higher false positive rate, but better protection against data leakage.
    /// </summary>
    High
}
