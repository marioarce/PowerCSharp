using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Internal;

/// <summary>A discovered module paired with its descriptor.</summary>
internal sealed record FeatureModuleEntry(IFeatureModule Module, FeatureDescriptor Descriptor);

/// <summary>
/// The ordered set of discovered modules plus diagnostics settings, carried from
/// <c>AddPowerFeatures</c> to <c>UsePowerFeatures</c> via a singleton.
/// </summary>
internal sealed class FeaturePipelinePlan
{
    public required IReadOnlyList<FeatureModuleEntry> Modules { get; init; }

    public bool DiagnosticsEndpointEnabled { get; init; }

    public string DiagnosticsPath { get; init; } = "/power-features";
}
