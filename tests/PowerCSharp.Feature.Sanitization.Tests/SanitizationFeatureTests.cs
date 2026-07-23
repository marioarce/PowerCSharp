using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Feature.Sanitization.Abstractions.NoOp;

namespace PowerCSharp.Feature.Sanitization.Tests;

/// <summary>
/// Covers <see cref="SanitizationFeatureExtensions.AddSanitizationFeature"/>, options binding, and
/// the <see cref="SanitizationEngineServiceProviderExtensions.ConfigureSanitizationEngine"/> bridge
/// into the static <see cref="SanitizationEngine"/>.
/// </summary>
public class SanitizationFeatureTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    private static ServiceCollection BaseServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public void AddSanitizationFeature_Registers_The_Real_Service()
    {
        var services = BaseServices();
        services.AddSanitizationFeature(BuildConfiguration());

        using var provider = services.BuildServiceProvider();

        Assert.IsType<SanitizationService>(provider.GetRequiredService<ISanitizationService>());
        Assert.IsType<SanitizationSettingsProvider>(provider.GetRequiredService<ISanitizationSettingsProvider>());
    }

    [Fact]
    public void Options_Bind_From_Configuration_Section()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Sanitization:Enabled", "true"),
            ("PowerFeatures:Sanitization:EnableLogSanitization", "false"),
            ("PowerFeatures:Sanitization:MaxSanitizedStringLength", "500"),
            ("PowerFeatures:Sanitization:SensitiveDataMaskCharacter", "#"));

        var services = BaseServices();
        services.AddSanitizationFeature(config);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SanitizationFeatureOptions>>().Value;

        Assert.True(options.Enabled);
        Assert.False(options.EnableLogSanitization);
        Assert.Equal(500, options.MaxSanitizedStringLength);
        Assert.Equal('#', options.SensitiveDataMaskCharacter);
    }

    [Fact]
    public void SettingsProvider_Reflects_Bound_Options()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Sanitization:EnableFilePathSanitization", "false"),
            ("PowerFeatures:Sanitization:MaxRegexPatternLength", "42"));

        var services = BaseServices();
        services.AddSanitizationFeature(config);

        using var provider = services.BuildServiceProvider();
        var settingsProvider = provider.GetRequiredService<ISanitizationSettingsProvider>();
        var settings = settingsProvider.GetCurrentSettings();

        Assert.False(settings.EnableFilePathSanitization);
        Assert.Equal(42, settings.MaxRegexPatternLength);
    }

    [Fact]
    public void DI_Resolved_Service_Honors_Bound_Options()
    {
        var config = BuildConfiguration(("PowerFeatures:Sanitization:EnableLogSanitization", "false"));

        var services = BaseServices();
        services.AddSanitizationFeature(config);

        using var provider = services.BuildServiceProvider();
        var sanitizer = provider.GetRequiredService<ISanitizationService>();

        var result = sanitizer.SanitizeForLogInjection("line1\r\nline2");

        Assert.False(result.WasModified);
        Assert.Equal("line1\r\nline2", result.SanitizedValue);
    }

    [Fact]
    public void NoOpSanitizationService_Passes_Every_Input_Through_Unchanged()
    {
        var services = BaseServices();
        services.TryAddSingleton<ISanitizationService, NoOpSanitizationService>();

        using var provider = services.BuildServiceProvider();
        var sanitizer = provider.GetRequiredService<ISanitizationService>();

        Assert.Equal("line1\r\nline2", sanitizer.SanitizeForLogInjection("line1\r\nline2").SanitizedValue);
        Assert.Equal("../../etc/passwd", sanitizer.SanitizeForFilePath("../../etc/passwd").SanitizedValue);
        Assert.Equal("password=secret", sanitizer.SanitizeForSensitiveData("password=secret").SanitizedValue);
        Assert.Equal("(a+)+$", sanitizer.SanitizeForRegexInjection("(a+)+$").SanitizedValue);
    }

    [Fact]
    public void ConfigureSanitizationEngine_Wires_The_Static_Engine_To_Reflect_Bound_Options()
    {
        var config = BuildConfiguration(("PowerFeatures:Sanitization:EnableLogSanitization", "false"));

        var services = BaseServices();
        services.AddSanitizationFeature(config);

        using var provider = services.BuildServiceProvider();

        try
        {
            provider.ConfigureSanitizationEngine();

            // No explicit settings passed: this now reflects the DI-resolved configuration.
            var result = SanitizationEngine.SanitizeForLogInjection("line1\r\nline2");

            Assert.False(result.WasModified);
            Assert.Equal("line1\r\nline2", result.SanitizedValue);
        }
        finally
        {
            // Reset ambient state so later tests default back to a neutral, out-of-the-box configuration.
            SanitizationEngine.SetConfigurationProvider(() => new SanitizationSettings());
        }
    }

    [Fact]
    public void ConfigureSanitizationEngine_Is_A_NoOp_When_No_Settings_Provider_Is_Registered()
    {
        var services = BaseServices();
        using var provider = services.BuildServiceProvider();

        var result = provider.ConfigureSanitizationEngine();

        Assert.Same(provider, result);
    }
}
