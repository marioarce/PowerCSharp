using Microsoft.Extensions.Configuration;
using PowerCSharp.Features.Abstractions;
using PowerCSharp.Features.Flags;
using Xunit;

namespace PowerCSharp.Features.Tests;

public class FlagResolutionTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    [Fact]
    public void Configuration_ReadsEnabledChild()
    {
        var config = BuildConfiguration(("PowerFeatures:Cache:Enabled", "true"));
        var provider = new ConfigurationFeatureFlagProvider(config);

        Assert.True(provider.IsEnabled("Cache"));
        Assert.Equal(FeatureFlagSource.Configuration, provider.GetValue("Cache").Source);
    }

    [Fact]
    public void Configuration_ReadsVariantLeaf()
    {
        var config = BuildConfiguration(("PowerFeatures:Cache:Provider", "BitFaster"));
        var provider = new ConfigurationFeatureFlagProvider(config);

        Assert.Equal("BitFaster", provider.GetValue("Cache:Provider").AsString());
    }

    [Fact]
    public void Composite_OverrideBeatsConfiguration()
    {
        var config = BuildConfiguration(("PowerFeatures:Cache:Enabled", "false"));
        var overrides = new OverrideFeatureFlagProvider(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Cache"] = "true" });

        var composite = new CompositeFeatureFlagProvider(new IFeatureFlagProvider[]
        {
            overrides,
            new ConfigurationFeatureFlagProvider(config),
        });

        var value = composite.GetValue("Cache");
        Assert.True(composite.IsEnabled("Cache"));
        Assert.Equal(FeatureFlagSource.Override, value.Source);
    }

    [Fact]
    public void Composite_FallsThroughToConfiguration()
    {
        var config = BuildConfiguration(("PowerFeatures:Cache:Enabled", "true"));
        var overrides = new OverrideFeatureFlagProvider(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        var composite = new CompositeFeatureFlagProvider(new IFeatureFlagProvider[]
        {
            overrides,
            new ConfigurationFeatureFlagProvider(config),
        });

        Assert.True(composite.IsEnabled("Cache"));
        Assert.Equal(FeatureFlagSource.Configuration, composite.GetValue("Cache").Source);
    }

    [Fact]
    public void Composite_MissingWhenNoSourceHasValue()
    {
        var composite = new CompositeFeatureFlagProvider(new IFeatureFlagProvider[]
        {
            new ConfigurationFeatureFlagProvider(BuildConfiguration()),
        });

        Assert.False(composite.IsEnabled("Unknown"));
        Assert.False(composite.GetValue("Unknown").HasValue);
    }
}
