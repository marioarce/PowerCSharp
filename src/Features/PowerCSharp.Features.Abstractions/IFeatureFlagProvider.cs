namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// Resolves feature-flag state. Not boolean-only: <see cref="GetValue"/> exposes typed
/// variants (string/enum/number) so flags can drive provider selection and multivariate config.
/// </summary>
public interface IFeatureFlagProvider
{
    /// <summary>Returns whether the feature identified by <paramref name="featureKey"/> is enabled.</summary>
    bool IsEnabled(string featureKey);

    /// <summary>Returns the typed value/variant for <paramref name="featureKey"/>.</summary>
    FeatureFlagValue GetValue(string featureKey);

    /// <summary>Asynchronous variant of <see cref="IsEnabled"/>.</summary>
    ValueTask<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default);

    /// <summary>Asynchronous variant of <see cref="GetValue"/>.</summary>
    ValueTask<FeatureFlagValue> GetValueAsync(string featureKey, CancellationToken cancellationToken = default);
}
