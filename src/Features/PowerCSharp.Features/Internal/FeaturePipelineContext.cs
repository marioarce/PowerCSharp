using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Internal;

/// <summary>Default <see cref="IFeaturePipelineContext"/> passed to a module when applying middleware.</summary>
internal sealed class FeaturePipelineContext : IFeaturePipelineContext
{
    public FeaturePipelineContext(
        IApplicationBuilder app,
        IConfiguration configuration,
        IFeatureFlagProvider flags,
        ILogger logger,
        FeatureDescriptor descriptor)
    {
        App = app;
        Configuration = configuration;
        Flags = flags;
        Logger = logger;
        Descriptor = descriptor;
    }

    public IApplicationBuilder App { get; }

    public IConfiguration Configuration { get; }

    public IFeatureFlagProvider Flags { get; }

    public ILogger Logger { get; }

    public FeatureDescriptor Descriptor { get; }
}
