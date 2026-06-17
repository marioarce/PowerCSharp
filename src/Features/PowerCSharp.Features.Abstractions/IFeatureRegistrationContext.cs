using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// The wiring surface passed to <see cref="IFeatureModule.ConfigureServices"/>.
/// Exposes the service collection, configuration, resolved flags, a logger and the descriptor.
/// </summary>
public interface IFeatureRegistrationContext
{
    /// <summary>The DI service collection the feature registers into.</summary>
    IServiceCollection Services { get; }

    /// <summary>The application configuration.</summary>
    IConfiguration Configuration { get; }

    /// <summary>Resolved flag access for conditional sub-registration.</summary>
    IFeatureFlagProvider Flags { get; }

    /// <summary>A logger scoped for feature registration diagnostics.</summary>
    ILogger Logger { get; }

    /// <summary>The descriptor for the feature being configured.</summary>
    FeatureDescriptor Descriptor { get; }
}
