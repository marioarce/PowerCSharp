using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Tests.TestSupport;

/// <summary>Marker recording the flag state observed when the module was configured.</summary>
public sealed record RecordingMarker(bool EnabledAtRegistration);

/// <summary>Test module that always registers a marker (verifies the self-gating model).</summary>
public sealed class RecordingFeatureModule : IFeatureModule
{
    public const string KeyName = "Recording";

    public string FeatureKey => KeyName;

    public int Order => 50;

    public void ConfigureServices(IFeatureRegistrationContext context)
    {
        context.Services.AddSingleton(new RecordingMarker(context.Flags.IsEnabled(FeatureKey)));
    }
}
