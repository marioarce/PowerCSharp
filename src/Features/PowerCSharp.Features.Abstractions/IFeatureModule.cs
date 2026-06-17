namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// The unit a feature implements to self-register into a host. Discovered by the engine
/// (auto-discovery) and/or invoked via an explicit <c>Add&lt;Name&gt;Feature()</c> extension.
/// </summary>
public interface IFeatureModule
{
    /// <summary>Stable identifier (e.g. <c>"Cache"</c>); matches <c>PowerFeatures:&lt;Key&gt;</c>.</summary>
    string FeatureKey { get; }

    /// <summary>Registration and middleware ordering; lower runs first.</summary>
    int Order { get; }

    /// <summary>Registers the feature's services into DI.</summary>
    void ConfigureServices(IFeatureRegistrationContext context);

    /// <summary>
    /// Registers the feature's middleware into the request pipeline.
    /// Default is a no-op for features that contribute no middleware.
    /// </summary>
    void ConfigurePipeline(IFeaturePipelineContext context)
    {
    }
}
