namespace PowerCSharp.Operational.Abstractions.Enums;

/// <summary>
/// Specifies the trace level for a diagnostic event, indicating its severity or importance.
/// Used to categorize and filter diagnostics events such as traces, debug information, warnings,
/// errors, and fatal events.
/// </summary>
public enum TraceLevel
{
    /// <summary>No trace level specified. Filtering treats this as "everything suppressed".</summary>
    None = 0,

    /// <summary>Detailed diagnostic information.</summary>
    Trace = 1,

    /// <summary>Debugging information.</summary>
    Debug = 2,

    /// <summary>General informational messages.</summary>
    Information = 3,

    /// <summary>A potential issue or noteworthy situation that is not an error.</summary>
    Warning = 4,

    /// <summary>An error that has occurred.</summary>
    Error = 5,

    /// <summary>A fatal or critical error that may cause the application to terminate.</summary>
    Fatal = 6
}
