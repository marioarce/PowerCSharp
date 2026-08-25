using PowerCSharp.Operational.Abstractions.Enums;
using PowerCSharp.Operational.Abstractions.Models;
using System.Runtime.CompilerServices;

namespace PowerCSharp.Operational.Abstractions;

/// <summary>
/// Defines a centralized mechanism for the application to capture errors, issues, and breadcrumbs,
/// enrich them with contextual data, and forward them to the in-app diagnostics service (and,
/// optionally, a third-party issue-tracking provider — see <c>PowerCSharp.Operational.Sentry</c>,
/// a later-phase package).
/// </summary>
/// <remarks>
/// Implementations must never throw as a result of capturing an issue — a failure to capture must
/// never surface as a failure of the operation being diagnosed.
/// </remarks>
public interface IIssueManager
{
    /// <summary>
    /// Captures an exception, enriches it with caller context, forwards it to diagnostics (and any
    /// registered provider), and returns the exception together with the resulting diagnostic event.
    /// </summary>
    /// <typeparam name="T">The type of exception being captured.</typeparam>
    /// <param name="ex">The exception instance to capture.</param>
    /// <param name="data">Optional additional data to associate with the exception.</param>
    /// <param name="member">The calling member name (supplied automatically by the compiler).</param>
    /// <param name="file">The calling file path (supplied automatically by the compiler).</param>
    /// <param name="line">The calling line number (supplied automatically by the compiler).</param>
    /// <returns>The exception (unmodified) and the resulting <see cref="DiagnosticEvent"/>, if diagnostics are enabled.</returns>
    (T Exception, DiagnosticEvent? DiagnosticEvent) CaptureException<T>(
        T ex,
        object? data = null,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        where T : Exception;

    /// <summary>
    /// Captures an error message (not backed by an exception), forwards it to diagnostics, and
    /// returns the resulting diagnostic event.
    /// </summary>
    /// <param name="message">The error message to capture.</param>
    /// <param name="data">Optional additional data to associate with the error.</param>
    /// <param name="member">The calling member name (supplied automatically by the compiler).</param>
    /// <param name="file">The calling file path (supplied automatically by the compiler).</param>
    /// <param name="line">The calling line number (supplied automatically by the compiler).</param>
    /// <returns>The resulting <see cref="DiagnosticEvent"/>, or <c>null</c> if diagnostics are disabled.</returns>
    DiagnosticEvent? CaptureError(
        string message,
        object? data = null,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0);

    /// <summary>
    /// Adds a breadcrumb to trace application flow leading up to a future error.
    /// </summary>
    /// <param name="message">The breadcrumb message.</param>
    /// <param name="category">The breadcrumb category. Defaults to <c>"general"</c>.</param>
    /// <param name="level">The breadcrumb level. Defaults to <see cref="BreadcrumbLevel.Info"/>.</param>
    /// <param name="data">Optional additional data to associate with the breadcrumb.</param>
    /// <returns>The resulting <see cref="DiagnosticEvent"/>, or <c>null</c> if diagnostics are disabled.</returns>
    DiagnosticEvent? AddBreadcrumb(string message, string category = "general", BreadcrumbLevel level = BreadcrumbLevel.Info, IDictionary<string, string>? data = null);
}
