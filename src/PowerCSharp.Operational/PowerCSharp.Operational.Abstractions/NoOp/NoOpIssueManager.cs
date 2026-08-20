using Microsoft.Extensions.Logging;
using PowerCSharp.Operational.Abstractions.Enums;
using PowerCSharp.Operational.Abstractions.Models;
using System.Runtime.CompilerServices;

namespace PowerCSharp.Operational.Abstractions.NoOp;

/// <summary>
/// Inert <see cref="IIssueManager"/> used when Operational is disabled. Exceptions are passed
/// through unmodified and no diagnostic event is produced, so dependents always resolve safely.
/// </summary>
public sealed class NoOpIssueManager : IIssueManager
{
    /// <summary>Creates the NoOp issue manager and logs that issue capture is inert.</summary>
    public NoOpIssueManager(ILogger<NoOpIssueManager> logger)
    {
        logger.LogInformation("Operational feature is disabled or unconfigured; using NoOp issue manager.");
    }

    /// <inheritdoc />
    public (T Exception, DiagnosticEvent? DiagnosticEvent) CaptureException<T>(
        T ex,
        object? data = null,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        where T : Exception
        => (ex, null);

    /// <inheritdoc />
    public DiagnosticEvent? CaptureError(
        string message,
        object? data = null,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        => null;

    /// <inheritdoc />
    public DiagnosticEvent? AddBreadcrumb(string message, string category = "general", BreadcrumbLevel level = BreadcrumbLevel.Info, IDictionary<string, string>? data = null) => null;
}
