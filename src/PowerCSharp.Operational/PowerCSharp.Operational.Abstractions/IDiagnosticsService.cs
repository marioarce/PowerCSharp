using PowerCSharp.Operational.Abstractions.Enums;
using PowerCSharp.Operational.Abstractions.Models;

namespace PowerCSharp.Operational.Abstractions;

/// <summary>
/// Defines the contract for the in-app diagnostics service: records trace events, breadcrumbs,
/// exceptions, and errors captured during a unit of work (typically an HTTP request), and returns
/// them as a filtered, sanitized payload for troubleshooting.
/// </summary>
/// <remarks>
/// Implementations are expected to be safe to call unconditionally — when diagnostics are not
/// enabled for the current context, event-adding methods are no-ops and return <c>null</c>, so
/// call sites never need to guard on <see cref="IsEnabled"/> first.
/// </remarks>
public interface IDiagnosticsService
{
    /// <summary>Gets a value indicating whether diagnostics are enabled for the current context.</summary>
    bool IsEnabled { get; }

    /// <summary>Gets a value indicating whether verbose diagnostics are enabled (disables obfuscation).</summary>
    bool IsVerbose { get; }

    /// <summary>Gets the minimum trace level events are recorded/returned at.</summary>
    int TraceLevel { get; }

    /// <summary>Gets a value indicating whether disk event-log writing is enabled for the current context.</summary>
    bool IsEventLogEnabled { get; }

    /// <summary>Gets a value indicating whether caching should be bypassed for the current context.</summary>
    bool IsCacheDisabled { get; }

    /// <summary>Gets a value indicating whether both debug and verbose diagnostics are enabled.</summary>
    bool IsDebugVerbose { get; }

    /// <summary>Gets a value indicating whether performance profiling is enabled for the current context.</summary>
    bool IsPerformanceEnabled { get; }

    /// <summary>
    /// Adds a trace event to the diagnostics log.
    /// </summary>
    /// <param name="message">The trace message to record.</param>
    /// <param name="level">The trace level. Defaults to <see cref="TraceLevel.Error"/>.</param>
    /// <param name="data">Optional additional data to associate with the event.</param>
    /// <param name="obfuscateMessage">Forces obfuscation of the message regardless of auto-detection.</param>
    /// <returns>The created <see cref="DiagnosticEvent"/>, or <c>null</c> if diagnostics are disabled.</returns>
    DiagnosticEvent? AddTrace(string message, TraceLevel level = Enums.TraceLevel.Error, object? data = null, bool obfuscateMessage = false);

    /// <summary>
    /// Adds a breadcrumb event to the diagnostics log.
    /// </summary>
    /// <param name="message">The breadcrumb message.</param>
    /// <param name="category">The breadcrumb category.</param>
    /// <param name="level">The breadcrumb level. Defaults to <see cref="BreadcrumbLevel.Info"/>.</param>
    /// <param name="obfuscateMessage">Forces obfuscation of the message regardless of auto-detection.</param>
    /// <returns>The created <see cref="DiagnosticEvent"/>, or <c>null</c> if diagnostics are disabled.</returns>
    DiagnosticEvent? AddBreadcrumb(string message, string category, BreadcrumbLevel level = BreadcrumbLevel.Info, bool obfuscateMessage = false);

    /// <summary>
    /// Adds an exception event to the diagnostics log.
    /// </summary>
    /// <param name="ex">The exception to record.</param>
    /// <param name="data">Optional additional data to associate with the event.</param>
    /// <returns>The created <see cref="DiagnosticEvent"/>, or <c>null</c> if diagnostics are disabled.</returns>
    DiagnosticEvent? AddException(Exception ex, object? data = null);

    /// <summary>
    /// Adds an error event to the diagnostics log.
    /// </summary>
    /// <param name="message">The error message to record.</param>
    /// <param name="data">Optional additional data to associate with the event.</param>
    /// <param name="obfuscateMessage">Forces obfuscation of the message regardless of auto-detection.</param>
    /// <returns>The created <see cref="DiagnosticEvent"/>, or <c>null</c> if diagnostics are disabled.</returns>
    DiagnosticEvent? AddError(string message, object? data = null, bool obfuscateMessage = false);

    /// <summary>
    /// Gets a filtered, sanitized snapshot of the diagnostic events recorded so far.
    /// </summary>
    /// <returns>The list of events, or <c>null</c> if diagnostics are disabled.</returns>
    List<DiagnosticEvent>? GetEvents();

    /// <summary>
    /// Builds a diagnostics payload from the current list of diagnostic events.
    /// </summary>
    /// <returns>The payload, or <c>null</c> if diagnostics are disabled.</returns>
    DiagnosticsPayload? BuildPayload();

    /// <summary>
    /// Obfuscates sensitive data in the provided input, unless verbose diagnostics are enabled.
    /// </summary>
    /// <param name="input">The object to obfuscate.</param>
    /// <returns>The obfuscated object, or the original input unchanged if no obfuscation is required.</returns>
    object? Obfuscate(object? input);
}
