using Microsoft.Extensions.Configuration;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Flags;

/// <summary>
/// Resolves flags from <c>appsettings</c> under the <c>PowerFeatures</c> section. A key may be a
/// scalar (<c>PowerFeatures:&lt;key&gt;</c>) or fall back to its <c>Enabled</c> child
/// (<c>PowerFeatures:&lt;key&gt;:Enabled</c>). Supports dotted variant keys (e.g. <c>Cache:Provider</c>).
/// </summary>
public sealed class ConfigurationFeatureFlagProvider : IFeatureFlagProvider
{
    /// <summary>The root configuration section for all feature flags.</summary>
    public const string SectionRoot = "PowerFeatures";

    private readonly IConfiguration _configuration;

    /// <summary>Creates the provider over the supplied configuration.</summary>
    public ConfigurationFeatureFlagProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public FeatureFlagValue GetValue(string featureKey)
    {
        var direct = _configuration[$"{SectionRoot}:{featureKey}"];
        if (direct is not null)
        {
            return new FeatureFlagValue(direct, FeatureFlagSource.Configuration);
        }

        var enabled = _configuration[$"{SectionRoot}:{featureKey}:Enabled"];
        return enabled is not null
            ? new FeatureFlagValue(enabled, FeatureFlagSource.Configuration)
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
