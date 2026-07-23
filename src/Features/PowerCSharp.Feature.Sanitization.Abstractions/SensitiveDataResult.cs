using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Represents the result of a sensitive-data detection and masking operation.
/// This type is immutable and designed for high-performance scenarios with minimal allocations.
/// </summary>
/// <remarks>
/// A plain class rather than a C# record, for the same <c>netstandard2.0</c> compatibility reason
/// documented on <see cref="SanitizationResult"/>.
/// </remarks>
public sealed class SensitiveDataResult : IEquatable<SensitiveDataResult>
{
    /// <summary>Gets the value after sensitive-data masking. Never null.</summary>
    public string SanitizedValue { get; }

    /// <summary>Gets a value indicating whether sensitive data was found and the original value was modified.</summary>
    public bool WasModified { get; }

    /// <summary>Gets the type of sanitization that was applied (always <see cref="SanitizationType.SensitiveData"/>).</summary>
    public SanitizationType SanitizationType { get; }

    /// <summary>Gets the time taken to perform the sensitive-data detection and masking.</summary>
    public TimeSpan ProcessingTime { get; }

    /// <summary>Gets a value indicating whether the input was rejected (always false for sensitive-data detection).</summary>
    public bool IsRejected { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SensitiveDataResult"/>. Prefer the
    /// <see cref="Unchanged"/> and <see cref="Modified"/> factory methods.
    /// </summary>
    /// <param name="sanitizedValue">The value after sensitive-data masking.</param>
    /// <param name="wasModified">Whether sensitive data was found and masked.</param>
    /// <param name="processingTime">The time taken to perform the detection and masking.</param>
    public SensitiveDataResult(string sanitizedValue, bool wasModified, TimeSpan processingTime)
    {
        SanitizedValue = sanitizedValue ?? string.Empty;
        WasModified = wasModified;
        SanitizationType = SanitizationType.SensitiveData;
        ProcessingTime = processingTime;
        IsRejected = false;
    }

    /// <summary>
    /// Gets a result that represents no sensitive data was found. Useful for fast-path scenarios
    /// where sensitive-data detection is not needed.
    /// </summary>
    /// <param name="originalValue">The original value that was not modified.</param>
    /// <returns>A <see cref="SensitiveDataResult"/> indicating no sensitive data was found.</returns>
    public static SensitiveDataResult Unchanged(string originalValue)
        => new(originalValue ?? string.Empty, wasModified: false, TimeSpan.Zero);

    /// <summary>
    /// Gets a result that represents sensitive data was found and masked.
    /// </summary>
    /// <param name="sanitizedValue">The value after masking sensitive data.</param>
    /// <param name="processingTime">The time taken to perform the detection and masking.</param>
    /// <returns>A <see cref="SensitiveDataResult"/> indicating sensitive data was found and masked.</returns>
    public static SensitiveDataResult Modified(string sanitizedValue, TimeSpan processingTime)
        => new(sanitizedValue ?? string.Empty, wasModified: true, processingTime);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SensitiveDataResult other && Equals(other);

    /// <inheritdoc />
    public bool Equals(SensitiveDataResult? other)
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
    public static bool operator ==(SensitiveDataResult? left, SensitiveDataResult? right)
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
    public static bool operator !=(SensitiveDataResult? left, SensitiveDataResult? right) => !(left == right);
}
