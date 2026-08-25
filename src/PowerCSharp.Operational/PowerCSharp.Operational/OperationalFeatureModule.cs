using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PowerCSharp.Features.Abstractions;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.NoOp;
using PowerCSharp.Operational.EventLog;
using PowerCSharp.Operational.Logging;
using PowerCSharp.Operational.Policies.Retry;
using System.Reflection;

namespace PowerCSharp.Operational;

/// <summary>
/// Optional <c>PowerCSharp.Features</c> auto-discovery module for Operational. This is entirely
/// optional — <see cref="OperationalServiceCollectionExtensions.AddOperational"/> registers
/// Operational without any dependency on the Features engine. Register this module (via
/// <c>options.ScanAssemblies(typeof(OperationalFeatureModule).Assembly)</c>) only if the host
/// already uses <c>PowerCSharp.Features</c> and wants Operational's enable/disable flag resolved
/// through the same composite provider chain (code override → custom flag provider → environment
/// variable → appsettings → default) as Cache and Sanitization.
/// </summary>
public sealed class OperationalFeatureModule : IFeatureModule
{
    /// <summary>The feature key: <c>PowerFeatures:Operational</c>.</summary>
    public const string Key = "Operational";

    /// <inheritdoc />
    public string FeatureKey => Key;

    /// <inheritdoc />
    public int Order => 0; // Cross-cutting: register ahead of leaf features that may want to log/capture during their own startup.

    /// <inheritdoc />
    public void ConfigureServices(IFeatureRegistrationContext context)
    {
        context.Services.Configure<OperationalOptions>(context.Configuration.GetSection($"PowerFeatures:{Key}"));
        context.Services.AddHttpContextAccessor();
        context.Services.TryAddSingleton<IEventViewerService, NoOpEventViewerService>();

        if (!context.Flags.IsEnabled(FeatureKey))
        {
            context.Logger.LogInformation("Operational feature disabled; registering NoOp diagnostics/issue-manager services.");
            context.Services.TryAddScoped<IDiagnosticsService, NoOpDiagnosticsService>();
            context.Services.TryAddScoped<IIssueManager, NoOpIssueManager>();
            return;
        }

        context.Services.TryAddSingleton<EventLogWriter>();
        context.Services.TryAddScoped<IDiagnosticsService, DiagnosticsService>();
        context.Services.TryAddScoped<IIssueManager, IssueManager>();
        context.Services.TryAddSingleton<IRetryPolicyProvider, RetryPolicyProvider>();
        context.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DiagnosticsLoggerProvider>());
    }
}
