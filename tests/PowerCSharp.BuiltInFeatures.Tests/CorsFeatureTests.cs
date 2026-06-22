using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.BuiltInFeatures.Cors;
using PowerCSharp.BuiltInFeatures.Tests.TestSupport;
using Xunit;

namespace PowerCSharp.BuiltInFeatures.Tests;

public class CorsFeatureTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    [Fact]
    public void Enabled_Registers_Cors_Services()
    {
        var config = BuildConfiguration(("PowerFeatures:Cors:Enabled", "true"));
        var services = new ServiceCollection();
        var context = new TestRegistrationContext(services, config, new StubFlags(true), CorsFeatureModule.Key);

        new CorsFeatureModule().ConfigureServices(context);

        Assert.Contains(services, d => d.ServiceType == typeof(ICorsService));
    }

    [Fact]
    public void Disabled_Skips_Cors_Registration()
    {
        var config = BuildConfiguration(("PowerFeatures:Cors:Enabled", "false"));
        var services = new ServiceCollection();
        var context = new TestRegistrationContext(services, config, new StubFlags(false), CorsFeatureModule.Key);

        new CorsFeatureModule().ConfigureServices(context);

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(ICorsService));
    }
}
