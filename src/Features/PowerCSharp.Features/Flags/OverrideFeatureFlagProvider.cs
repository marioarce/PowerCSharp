using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Flags;

/// <summary>
/// Highest-precedence provider backed by explicit code overrides supplied by the host
/// in the <c>AddPowerFeatures</c> callback.
/// </summary>
public sealed class OverrideFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly IReadOnlyDictionary<string, string> _overrides;

    /// <summary>Creates the provider over the supplied override map (case-insensitive keys).</summary>
    public OverrideFeatureFlagProvider(IReadOnlyDictionary<string, string> overrides)
    {
        _overrides = overrides;
    }

    /// <inheritdoc />
    public FeatureFlagValue GetValue(string featureKey)
        => _overrides.TryGetValue(featureKey, out var value)
            ? new FeatureFlagValue(value, FeatureFlagSource.Override)
            : FeatureFlagValue.Missing;

    /// <inheritdoc />
    public bool IsEnabled(string featureKey)
    {
        var value = GetValue(featureKey);
        return value.HasValue && value.AsBoolean();
    }

    /// <inheritdoc />
    public ValueTask<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default)
        => new(IsEnabled(featureKey));

    /// <inheritdoc />
    public ValueTask<FeatureFlagValue> GetValueAsync(string featureKey, CancellationToken cancellationToken = default)
        => new(GetValue(featureKey));
}
