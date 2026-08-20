using Microsoft.Extensions.Logging;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.Enums;
using PowerCSharp.Operational.Abstractions.Models;
using System.Runtime.CompilerServices;

namespace PowerCSharp.Operational;

/// <summary>
/// Centralized mechanism for the application to capture errors, issues, and breadcrumbs. Enriches
/// each capture with caller context and forwards it to <see cref="IDiagnosticsService"/>.
/// </summary>
/// <remarks>
/// This is the "agnostic to the service" piece of Operational: it offers a single, shared capture
/// surface, and forwards captures to whatever downstream provider is registered. In this build the
/// only downstream target is <see cref="IDiagnosticsService"/> (in-app diagnostics); a future
/// <c>PowerCSharp.Operational.Sentry</c> provider package can register itself to receive the same
/// captures, following the pattern established by <c>PowerCSharp.Feature.Cache.BitFaster</c>.
/// </remarks>
public sealed class IssueManager : IIssueManager
{
    private readonly IDiagnosticsService _diagnosticsService;
    private readonly ILogger<IssueManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueManager"/> class.
    /// </summary>
    /// <param name="diagnosticsService">The diagnostics service captures are forwarded to.</param>
    /// <param name="logger">The logger used to record capture failures (never lets a capture-time exception escape).</param>
    public IssueManager(IDiagnosticsService diagnosticsService, ILogger<IssueManager> logger)
    {
        ArgumentNullException.ThrowIfNull(diagnosticsService);
        ArgumentNullException.ThrowIfNull(logger);

        _diagnosticsService = diagnosticsService;
        _logger = logger;
    }

    /// <inheritdoc />
    public (T Exception, DiagnosticEvent? DiagnosticEvent) CaptureException<T>(
        T ex,
        object? data = null,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        where T : Exception
    {
        ArgumentNullException.ThrowIfNull(ex);

        // TODO: attach the request/operation correlation id here once PowerCSharp ships a
        // correlation-id abstraction (no equivalent exists yet in PowerCSharp — flagged in the
        // architecture plan as a known v1 gap, not silently dropped).

        if (data is IDictionary<string, object> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                ex.Data[kvp.Key] = kvp.Value;
            }
        }

        DiagnosticEvent? result = null;

        try
        {
            result = _diagnosticsService.AddException(ex, new
            {
                data,
                caller = $"{file}:{line} ({member})"
            });
        }
        catch (Exception captureEx)
        {
            // A failure to capture must never surface as a failure of the operation being
            // diagnosed — log it and move on.
            _logger.LogWarning(captureEx, "IssueManager failed to forward exception to diagnostics.");
        }

        return (ex, result);
    }

    /// <inheritdoc />
    public DiagnosticEvent? CaptureError(
        string message,
        object? data = null,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageData = new Dictionary<string, object?>
        {
            ["caller"] = $"{file}:{line} ({member})"
        };

        if (data != null)
        {
            messageData["data"] = data;
        }

        try
        {
            return _diagnosticsService.AddError(message, messageData);
        }
        catch (Exception captureEx)
        {
            _logger.LogWarning(captureEx, "IssueManager failed to forward error to diagnostics.");
            return null;
        }
    }

    /// <inheritdoc />
    public DiagnosticEvent? AddBreadcrumb(string message, string category = "general", BreadcrumbLevel level = BreadcrumbLevel.Info, IDictionary<string, string>? data = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            return _diagnosticsService.AddBreadcrumb(message, category);
        }
        catch (Exception captureEx)
        {
            _logger.LogWarning(captureEx, "IssueManager failed to forward breadcrumb to diagnostics.");
            return null;
        }
    }
}
