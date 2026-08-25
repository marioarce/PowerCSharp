using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.Models;
using TraceLevel = PowerCSharp.Operational.Abstractions.Enums.TraceLevel;

namespace PowerCSharp.Operational.Logging;

/// <summary>
/// A custom <see cref="ILogger"/> that forwards log messages and exceptions to
/// <see cref="IDiagnosticsService"/> (in-app diagnostics) and, if registered, an
/// <see cref="IEventViewerService"/> (platform event log).
/// </summary>
/// <remarks>
/// <see cref="IDiagnosticsService"/> is scoped per-request, but <c>ILoggerProvider.CreateLogger</c>
/// runs once at host startup, outside any request scope — so this class deliberately resolves
/// <see cref="IDiagnosticsService"/> from <c>HttpContext.RequestServices</c> at each
/// <see cref="Log{TState}"/> call rather than through constructor injection. This is the standard
/// pattern for bridging a singleton-lifetime <c>ILogger</c> to per-request scoped services and is
/// not the same as a general-purpose service locator.
/// </remarks>
public sealed class DiagnosticsLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventViewerService _eventViewerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsLogger"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context and its request-scoped services.</param>
    /// <param name="eventViewerService">The event-viewer forwarding hook (NoOp by default).</param>
    /// <param name="categoryName">The logger category name.</param>
    public DiagnosticsLogger(IHttpContextAccessor httpContextAccessor, IEventViewerService eventViewerService, string categoryName)
    {
        _httpContextAccessor = httpContextAccessor;
        _eventViewerService = eventViewerService;
        _categoryName = categoryName;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        var headers = _httpContextAccessor.HttpContext?.Request?.Headers;

        if (headers?.TryGetValue(DiagnosticHeaders.TraceLevel, out var traceLevelValues) ?? false)
        {
            if (int.TryParse(traceLevelValues.FirstOrDefault(), out var traceLevel) && traceLevel is >= 0 and <= 5)
            {
                return logLevel >= MapTraceLevelToLogLevel(traceLevel);
            }
        }

        return logLevel >= DiagnosticsService.DefaultLogLevel;
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        string message;
        try
        {
            message = formatter(state, exception);
        }
        catch
        {
            message = "[No message]";
        }

        // Sanitize for log injection (CWE-117) and sensitive-data exposure (CWE-200) before this
        // message goes anywhere — diagnostics, disk, or the platform event log.
        message = message.SanitizeForLog().SanitizeForSensitiveData();

        var diagnosticsService = _httpContextAccessor.HttpContext?.RequestServices?.GetService<IDiagnosticsService>();

        if (diagnosticsService?.IsEnabled ?? false)
        {
            if (exception != null)
            {
                diagnosticsService.AddException(exception, new { message, category = _categoryName });
            }
            else
            {
                diagnosticsService.AddTrace(message, MapLogLevelToTraceLevel(logLevel), new { category = _categoryName });
            }
        }

        ForwardToEventViewer(logLevel, message, exception);
    }

    private static TraceLevel MapLogLevelToTraceLevel(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => TraceLevel.Trace,
        LogLevel.Debug => TraceLevel.Debug,
        LogLevel.Information => TraceLevel.Information,
        LogLevel.Warning => TraceLevel.Warning,
        LogLevel.Error => TraceLevel.Error,
        LogLevel.Critical => TraceLevel.Fatal,
        _ => TraceLevel.Information
    };

    private static LogLevel MapTraceLevelToLogLevel(int traceLevel) => traceLevel switch
    {
        1 => LogLevel.Debug,
        2 => LogLevel.Information,
        3 => LogLevel.Warning,
        4 => LogLevel.Error,
        5 => LogLevel.Critical,
        6 => LogLevel.None,
        _ => LogLevel.Warning
    };

    /// <summary>
    /// Forwards the log entry to <see cref="IEventViewerService"/>. Safe by construction: the
    /// default registration is <c>NoOpEventViewerService</c>, so this is inert unless a provider
    /// package (e.g. a future <c>PowerCSharp.Operational.WinEventLog</c>) overrides it.
    /// </summary>
    private void ForwardToEventViewer(LogLevel logLevel, string sanitizedMessage, Exception? exception)
    {
        try
        {
            var entry = new EventViewerLogEntry(
                Timestamp: DateTime.UtcNow,
                Category: _categoryName,
                Level: logLevel,
                Message: sanitizedMessage,
                CorrelationId: null, // TODO: populate once PowerCSharp ships a correlation-id abstraction.
                ExceptionText: exception?.ToString().SanitizeForLog());

            _eventViewerService.TryEnqueue(entry);
        }
        catch
        {
            // A failure to forward to the event log must never fail the log call itself.
        }
    }
}
