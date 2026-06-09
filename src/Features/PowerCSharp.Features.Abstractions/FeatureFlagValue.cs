using System.Globalization;

namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// A resolved feature-flag value. Flags are not limited to on/off: the raw value can be
/// interpreted as a boolean, string, number or enum variant (e.g. provider selection),
/// and carries the <see cref="Source"/> that produced it for diagnostics.
/// </summary>
public sealed class FeatureFlagValue
{
    /// <summary>A value representing "not found", attributed to <see cref="FeatureFlagSource.Default"/>.</summary>
    public static readonly FeatureFlagValue Missing = new(null, FeatureFlagSource.Default);

    /// <summary>Creates a value from a raw string and the source that produced it.</summary>
    public FeatureFlagValue(string? rawValue, FeatureFlagSource source)
    {
        RawValue = rawValue;
        Source = source;
    }

    /// <summary>The raw string value, or <c>null</c> when absent.</summary>
    public string? RawValue { get; }

    /// <summary>Where this value came from.</summary>
    public FeatureFlagSource Source { get; }

    /// <summary>Whether a value was actually present.</summary>
    public bool HasValue => RawValue is not null;

    /// <summary>Interprets the value as a boolean, falling back to <paramref name="defaultValue"/>.</summary>
    public bool AsBoolean(bool defaultValue = false)
        => bool.TryParse(RawValue, out var result) ? result : defaultValue;

    /// <summary>Returns the raw string value.</summary>
    public string? AsString() => RawValue;

    /// <summary>Interprets the value as an <see cref="int"/>, falling back to <paramref name="defaultValue"/>.</summary>
    public int AsInt32(int defaultValue = 0)
        => int.TryParse(RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;

    /// <summary>Interprets the value as the enum <typeparamref name="T"/> (case-insensitive).</summary>
    public T AsEnum<T>(T defaultValue = default) where T : struct, Enum
        => Enum.TryParse<T>(RawValue, ignoreCase: true, out var result) ? result : defaultValue;

    /// <summary>Creates a boolean-backed value with an explicit source.</summary>
    public static FeatureFlagValue For(bool enabled, FeatureFlagSource source)
        => new(enabled ? "true" : "false", source);

    /// <inheritdoc />
    public override string ToString() => $"{RawValue ?? "<null>"} (source: {Source})";
}
