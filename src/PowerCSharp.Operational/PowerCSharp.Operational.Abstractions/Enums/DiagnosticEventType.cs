using System.Text.Json.Serialization;

namespace PowerCSharp.Operational.Abstractions.Enums;

/// <summary>
/// Specifies the type of a diagnostic event captured during application execution.
/// Used to categorize events such as traces, breadcrumbs, exceptions, and errors for diagnostics
/// and logging purposes.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DiagnosticEventType>))]
public enum DiagnosticEventType
{
    /// <summary>A trace event, typically used for general diagnostic information.</summary>
    Trace,

    /// <summary>A breadcrumb event, used to record a checkpoint in application flow.</summary>
    Breadcrumb,

    /// <summary>An exception event, used to capture and log exceptions.</summary>
    Exception,

    /// <summary>An error event, used to log a problem that is not an exception.</summary>
    Error
}
