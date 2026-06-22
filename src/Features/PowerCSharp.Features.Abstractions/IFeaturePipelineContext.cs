using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// The wiring surface passed to <see cref="IFeatureModule.ConfigurePipeline"/> for
/// middleware-bearing features.
/// </summary>
public interface IFeaturePipelineContext
{
    /// <summary>The application builder used to register middleware.</summary>
    IApplicationBuilder App { get; }

    /// <summary>The application configuration.</summary>
    IConfiguration Configuration { get; }

    /// <summary>Resolved flag access.</summary>
    IFeatureFlagProvider Flags { get; }

    /// <summary>A logger scoped for pipeline diagnostics.</summary>
    ILogger Logger { get; }

    /// <summary>The descriptor for the feature being configured.</summary>
    FeatureDescriptor Descriptor { get; }
}
