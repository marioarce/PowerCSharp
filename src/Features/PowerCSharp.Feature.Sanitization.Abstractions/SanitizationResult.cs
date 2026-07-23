using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Represents the result of a sanitization operation, providing details about the processing outcome.
/// This type is immutable and designed for high-performance scenarios with minimal allocations.
/// </summary>
/// <remarks>
/// A plain class rather than a C# record: this package targets <c>netstandard2.0</c>, which lacks
/// the <c>init</c>-accessor support records rely on. Value equality is implemented manually instead,
/// matching the convention already used by <c>PowerCSharp.Feature.Cache.Abstractions.CacheResult&lt;T&gt;</c>.
/// </remarks>
public sealed class SanitizationResult : IEquatable<SanitizationResult>
{
    /// <summary>Gets the sanitized value after processing. Never null.</summary>
    public string SanitizedValue { get; }

    /// <summary>Gets a value indicating whether the original value was modified during sanitization.</summary>
    public bool WasModified { get; }

    /// <summary>Gets the type of sanitization that was applied.</summary>
    public SanitizationType SanitizationType { get; }

    /// <summary>Gets the time taken to perform the sanitization operation.</summary>
    public TimeSpan ProcessingTime { get; }

    /// <summary>Gets a value indicating whether the input was rejected due to validation failures (strict mode only).</summary>
    public bool IsRejected { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SanitizationResult"/>. Prefer the
    /// <see cref="Unchanged"/>, <see cref="Modified"/>, and <see cref="Rejected"/> factory methods.
    /// </summary>
    /// <param name="sanitizedValue">The sanitized value after processing.</param>
    /// <param name="wasModified">Whether the original value was modified.</param>
    /// <param name="sanitizationType">The type of sanitization that was applied.</param>
    /// <param name="processingTime">The time taken to perform the sanitization operation.</param>
    /// <param name="isRejected">Whether the input was rejected due to validation failures.</param>
    public SanitizationResult(
        string sanitizedValue,
        bool wasModified,
        SanitizationType sanitizationType,
        TimeSpan processingTime,
        bool isRejected = false)
    {
        SanitizedValue = sanitizedValue ?? string.Empty;
        WasModified = wasModified;
        SanitizationType = sanitizationType;
        ProcessingTime = processingTime;
        IsRejected = isRejected;
    }

    /// <summary>
    /// Gets a sanitized result that represents no changes were made. Useful for fast-path
    /// scenarios where sanitization is not needed.
    /// </summary>
    /// <param name="originalValue">The original value that was not modified.</param>
    /// <param name="sanitizationType">The type of sanitization that was checked.</param>
    /// <returns>A <see cref="SanitizationResult"/> indicating no modifications were made.</returns>
    public static SanitizationResult Unchanged(string originalValue, SanitizationType sanitizationType)
        => new(originalValue ?? string.Empty, wasModified: false, sanitizationType, TimeSpan.Zero);

    /// <summary>
    /// Gets a sanitized result that represents successful modification.
    /// </summary>
    /// <param name="sanitizedValue">The sanitized value after processing.</param>
    /// <param name="sanitizationType">The type of sanitization that was applied.</param>
    /// <param name="processingTime">The time taken to perform the sanitization.</param>
    /// <returns>A <see cref="SanitizationResult"/> indicating successful modification.</returns>
    public static SanitizationResult Modified(
        string sanitizedValue,
        SanitizationType sanitizationType,
        TimeSpan processingTime)
        => new(sanitizedValue ?? string.Empty, wasModified: true, sanitizationType, processingTime);

    /// <summary>
    /// Gets a sanitized result that represents input rejection due to validation failures.
    /// Used in strict validation mode when inputs fail allowlist validation.
    /// </summary>
    /// <param name="sanitizationType">The type of sanitization that was applied.</param>
    /// <param name="processingTime">The time taken to perform the validation.</param>
    /// <returns>A <see cref="SanitizationResult"/> indicating the input was rejected.</returns>
    public static SanitizationResult Rejected(
        SanitizationType sanitizationType,
        TimeSpan processingTime)
        => new(string.Empty, wasModified: true, sanitizationType, processingTime, isRejected: true);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SanitizationResult other && Equals(other);

    /// <inheritdoc />
    public bool Equals(SanitizationResult? other)
    {
        if (other is null)
        {
            return false;
        }

        return WasModified == other.WasModified &&
               SanitizationType == other.SanitizationType &&
               IsRejected == other.IsRejected &&
               ProcessingTime.Equals(other.ProcessingTime) &&
               string.Equals(SanitizedValue, other.SanitizedValue, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;
        hash = hash * 31 + SanitizedValue.GetHashCode();
        hash = hash * 31 + WasModified.GetHashCode();
        hash = hash * 31 + SanitizationType.GetHashCode();
        hash = hash * 31 + ProcessingTime.GetHashCode();
        hash = hash * 31 + IsRejected.GetHashCode();
        return hash;
    }

    /// <summary>Equality operator.</summary>
    public static bool operator ==(SanitizationResult? left, SanitizationResult? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(SanitizationResult? left, SanitizationResult? right) => !(left == right);
}
