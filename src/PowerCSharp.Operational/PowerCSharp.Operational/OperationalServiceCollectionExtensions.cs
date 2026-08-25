using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.NoOp;
using PowerCSharp.Operational.EventLog;
using PowerCSharp.Operational.Logging;
using PowerCSharp.Operational.Policies.Retry;
using System.Reflection;

namespace PowerCSharp.Operational;

/// <summary>
/// Explicit (no-reflection) DI registration for PowerCSharp.Operational. Works standalone — no
/// dependency on <c>PowerCSharp.Features</c> is required to use these extensions; see
/// <see cref="OperationalFeatureModule"/> for the optional Features Framework auto-discovery path.
/// </summary>
public static class OperationalServiceCollectionExtensions
{
    /// <summary>
    /// Registers PowerCSharp.Operational. Binds <see cref="OperationalOptions"/> from
    /// <c>PowerFeatures:Operational</c>. When <see cref="OperationalOptions.Enabled"/> is
    /// <c>false</c> (default <c>true</c>), registers the NoOp floors from
    /// <c>PowerCSharp.Operational.Abstractions</c> for every contract — no background writer task
    /// starts, no custom <see cref="ILoggerProvider"/> is added, and the host behaves exactly as if
    /// Operational were never referenced.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    public static IServiceCollection AddOperational(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection("PowerFeatures:Operational");
        services.Configure<OperationalOptions>(section);
        services.AddHttpContextAccessor();

        // Safe-off floor: NoOp is registered unless a provider package (e.g. a future
        // PowerCSharp.Operational.WinEventLog) supplies a concrete implementation.
        services.TryAddSingleton<IEventViewerService, NoOpEventViewerService>();

        var enabled = section.Get<OperationalOptions>()?.Enabled ?? true;

        if (enabled)
        {
            services.TryAddSingleton<EventLogWriter>();
            services.TryAddScoped<IDiagnosticsService, DiagnosticsService>();
            services.TryAddScoped<IIssueManager, IssueManager>();
            services.TryAddSingleton<IRetryPolicyProvider, RetryPolicyProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DiagnosticsLoggerProvider>());
        }
        else
        {
            services.TryAddScoped<IDiagnosticsService, NoOpDiagnosticsService>();
            services.TryAddScoped<IIssueManager, NoOpIssueManager>();
            // EventLogWriter, DiagnosticsLoggerProvider, and RetryPolicyProvider are deliberately
            // left unregistered when disabled — nothing starts a background task or hooks logging.
        }

        return services;
    }

    /// <summary>
    /// Completes Operational startup: eagerly starts the <see cref="EventLogWriter"/> background
    /// task and kicks off one round of disk event-log retention cleanup. Call once, during app
    /// startup, after <see cref="AddOperational"/>.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static IApplicationBuilder UseOperational(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var services = app.ApplicationServices;
        var options = services.GetRequiredService<IOptions<OperationalOptions>>().Value;

        if (!options.Enabled)
        {
            return app; // Nothing to start — NoOp floors were registered instead.
        }

        // Eagerly resolve EventLogWriter so its background queue-processing task starts at
        // startup rather than lazily on the first diagnostic event.
        _ = services.GetRequiredService<EventLogWriter>();

        var appName = options.AppName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "App";
        var logger = services.GetService<ILogger<EventLogWriter>>();

        EventLogRetentionCleaner.RunRetentionCleanup(options.LogsBasePath, appName, options.LogsRetentionDays, logger);

        return app;
    }
}
