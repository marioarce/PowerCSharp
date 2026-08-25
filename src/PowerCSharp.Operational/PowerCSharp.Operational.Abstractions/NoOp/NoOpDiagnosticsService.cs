using Microsoft.Extensions.Logging;
using PowerCSharp.Operational.Abstractions.Enums;
using PowerCSharp.Operational.Abstractions.Models;

namespace PowerCSharp.Operational.Abstractions.NoOp;

/// <summary>
/// Inert <see cref="IDiagnosticsService"/> used when Operational is disabled. Every add-event call
/// is a no-op returning <c>null</c>, diagnostics are always reported as disabled, and
/// <see cref="Obfuscate"/> returns the input unchanged — so dependents always resolve safely and
/// the host application behaves exactly as if Operational were never referenced.
/// </summary>
public sealed class NoOpDiagnosticsService : IDiagnosticsService
{
    /// <summary>Creates the NoOp diagnostics service and logs that diagnostics are inert.</summary>
    public NoOpDiagnosticsService(ILogger<NoOpDiagnosticsService> logger)
    {
        logger.LogInformation("Operational feature is disabled or unconfigured; using NoOp diagnostics service.");
    }

    /// <inheritdoc />
    public bool IsEnabled => false;

    /// <inheritdoc />
    public bool IsVerbose => false;

    /// <inheritdoc />
    public int TraceLevel => (int)Enums.TraceLevel.None;

    /// <inheritdoc />
    public bool IsEventLogEnabled => false;

    /// <inheritdoc />
    public bool IsCacheDisabled => false;

    /// <inheritdoc />
    public bool IsDebugVerbose => false;

    /// <inheritdoc />
    public bool IsPerformanceEnabled => false;

    /// <inheritdoc />
    public DiagnosticEvent? AddTrace(string message, TraceLevel level = Enums.TraceLevel.Error, object? data = null, bool obfuscateMessage = false) => null;

    /// <inheritdoc />
    public DiagnosticEvent? AddBreadcrumb(string message, string category, BreadcrumbLevel level = BreadcrumbLevel.Info, bool obfuscateMessage = false) => null;

    /// <inheritdoc />
    public DiagnosticEvent? AddException(Exception ex, object? data = null) => null;

    /// <inheritdoc />
    public DiagnosticEvent? AddError(string message, object? data = null, bool obfuscateMessage = false) => null;

    /// <inheritdoc />
    public List<DiagnosticEvent>? GetEvents() => null;

    /// <inheritdoc />
    public DiagnosticsPayload? BuildPayload() => null;

    /// <inheritdoc />
    public object? Obfuscate(object? input) => input;
}
