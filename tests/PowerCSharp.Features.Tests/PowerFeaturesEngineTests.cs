using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Features.Abstractions;
using PowerCSharp.Features.Registry;
using PowerCSharp.Features.Tests.TestSupport;
using Xunit;

namespace PowerCSharp.Features.Tests;

public class PowerFeaturesEngineTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    private static ServiceProvider BuildProvider(IConfiguration configuration, Action<PowerFeaturesOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddPowerFeatures(configuration, options =>
        {
            options.ScanAssemblies(typeof(RecordingFeatureModule).Assembly);
            configure?.Invoke(options);
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Discovers_And_Enables_From_Configuration()
    {
        var config = BuildConfiguration(("PowerFeatures:Recording:Enabled", "true"));
        using var provider = BuildProvider(config);

        var marker = provider.GetRequiredService<RecordingMarker>();
        Assert.True(marker.EnabledAtRegistration);

        var entry = Assert.Single(
            provider.GetRequiredService<FeatureRegistry>().Entries,
            e => e.Key == RecordingFeatureModule.KeyName);
        Assert.True(entry.Enabled);
        Assert.Equal(FeatureFlagSource.Configuration, entry.Source);
    }

    [Fact]
    public void ConfigureServices_Invoked_Even_When_Disabled()
    {
        var config = BuildConfiguration(("PowerFeatures:Recording:Enabled", "false"));
        using var provider = BuildProvider(config);

        // Self-gating model: the module runs and observes the disabled flag.
        var marker = provider.GetRequiredService<RecordingMarker>();
        Assert.False(marker.EnabledAtRegistration);

        var entry = Assert.Single(
            provider.GetRequiredService<FeatureRegistry>().Entries,
            e => e.Key == RecordingFeatureModule.KeyName);
        Assert.False(entry.Enabled);
    }

    [Fact]
    public void Override_Wins_Over_Configuration()
    {
        var config = BuildConfiguration(("PowerFeatures:Recording:Enabled", "false"));
        using var provider = BuildProvider(config, o => o.Override(RecordingFeatureModule.KeyName, true));

        var marker = provider.GetRequiredService<RecordingMarker>();
        Assert.True(marker.EnabledAtRegistration);

        var entry = Assert.Single(
            provider.GetRequiredService<FeatureRegistry>().Entries,
            e => e.Key == RecordingFeatureModule.KeyName);
        Assert.True(entry.Enabled);
        Assert.Equal(FeatureFlagSource.Override, entry.Source);
    }

    [Fact]
    public void Registers_Composite_Flag_Provider()
    {
        using var provider = BuildProvider(BuildConfiguration());
        Assert.NotNull(provider.GetRequiredService<IFeatureFlagProvider>());
    }
}
