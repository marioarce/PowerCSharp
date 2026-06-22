using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Flags;

/// <summary>
/// Resolves flags from environment variables using the convention
/// <c>POWERFEATURES__&lt;KEY&gt;</c> (and <c>POWERFEATURES__&lt;KEY&gt;__ENABLED</c>).
/// Dotted variant keys map <c>:</c> to <c>__</c> (e.g. <c>Cache:Provider</c> → <c>POWERFEATURES__CACHE__PROVIDER</c>).
/// </summary>
public sealed class EnvironmentFeatureFlagProvider : IFeatureFlagProvider
{
    /// <summary>The environment-variable prefix for all feature flags.</summary>
    public const string Prefix = "POWERFEATURES__";

    /// <inheritdoc />
    public FeatureFlagValue GetValue(string featureKey)
    {
        var envKey = Prefix + featureKey.ToUpperInvariant().Replace(":", "__");

        var direct = Environment.GetEnvironmentVariable(envKey);
        if (direct is not null)
        {
            return new FeatureFlagValue(direct, FeatureFlagSource.Environment);
        }

        var enabled = Environment.GetEnvironmentVariable(envKey + "__ENABLED");
        return enabled is not null
            ? new FeatureFlagValue(enabled, FeatureFlagSource.Environment)
            : FeatureFlagValue.Missing;
    }

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
