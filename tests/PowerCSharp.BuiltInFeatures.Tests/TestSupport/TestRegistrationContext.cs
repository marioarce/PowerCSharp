using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.BuiltInFeatures.Tests.TestSupport;

/// <summary>Minimal flag stub returning a fixed enabled state.</summary>
public sealed class StubFlags : IFeatureFlagProvider
{
    private readonly bool _enabled;

    public StubFlags(bool enabled) => _enabled = enabled;

    public bool IsEnabled(string featureKey) => _enabled;

    public FeatureFlagValue GetValue(string featureKey) => FeatureFlagValue.For(_enabled, FeatureFlagSource.Override);

    public ValueTask<bool> IsEnabledAsync(string featureKey, CancellationToken cancellationToken = default) => new(_enabled);

    public ValueTask<FeatureFlagValue> GetValueAsync(string featureKey, CancellationToken cancellationToken = default) => new(GetValue(featureKey));
}

/// <summary>Test double for <see cref="IFeatureRegistrationContext"/>.</summary>
public sealed class TestRegistrationContext : IFeatureRegistrationContext
{
    public TestRegistrationContext(IServiceCollection services, IConfiguration configuration, IFeatureFlagProvider flags, string featureKey)
    {
        Services = services;
        Configuration = configuration;
        Flags = flags;
        Logger = NullLogger.Instance;
        Descriptor = new FeatureDescriptor { Key = featureKey, Tier = FeatureTier.BuiltIn };
    }

    public IServiceCollection Services { get; }

    public IConfiguration Configuration { get; }

    public IFeatureFlagProvider Flags { get; }

    public ILogger Logger { get; }

    public FeatureDescriptor Descriptor { get; }
}
