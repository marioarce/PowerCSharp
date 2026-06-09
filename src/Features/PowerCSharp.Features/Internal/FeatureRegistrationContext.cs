using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Internal;

/// <summary>Default <see cref="IFeatureRegistrationContext"/> passed to a module at registration.</summary>
internal sealed class FeatureRegistrationContext : IFeatureRegistrationContext
{
    public FeatureRegistrationContext(
        IServiceCollection services,
        IConfiguration configuration,
        IFeatureFlagProvider flags,
        ILogger logger,
        FeatureDescriptor descriptor)
    {
        Services = services;
        Configuration = configuration;
        Flags = flags;
        Logger = logger;
        Descriptor = descriptor;
    }

    public IServiceCollection Services { get; }

    public IConfiguration Configuration { get; }

    public IFeatureFlagProvider Flags { get; }

    public ILogger Logger { get; }

    public FeatureDescriptor Descriptor { get; }
}
