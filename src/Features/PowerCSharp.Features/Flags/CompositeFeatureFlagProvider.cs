using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Flags;

/// <summary>
/// Composes registered providers into a single resolver. Providers are consulted in
/// precedence order (highest first); the first provider that yields a value wins. This is
/// the <see cref="IFeatureFlagProvider"/> registered for runtime resolution.
/// </summary>
public sealed class CompositeFeatureFlagProvider : IFeatureFlagProvider
{
    private readonly IReadOnlyList<IFeatureFlagProvider> _providers;

    /// <summary>Creates the composite over the supplied providers, highest precedence first.</summary>
    public CompositeFeatureFlagProvider(IEnumerable<IFeatureFlagProvider> providersHighestFirst)
    {
        _providers = providersHighestFirst.ToList();
    }

    /// <inheritdoc />
    public FeatureFlagValue GetValue(string featureKey)
    {
        foreach (var provider in _providers)
        {
            var value = provider.GetValue(featureKey);
            if (value.HasValue)
            {
                return value;
            }
        }

        return FeatureFlagValue.Missing;
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
