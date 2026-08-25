using PowerCSharp.Operational.Abstractions.Enums;
using System.Text.Json.Serialization;

namespace PowerCSharp.Operational.Abstractions.Models;

/// <summary>
/// Represents a single diagnostic event — a trace, error, exception, or breadcrumb — captured
/// during application execution. Captures the timestamp, type, message, stack trace, additional
/// data, and trace level for the event.
/// </summary>
public class DiagnosticEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticEvent"/> class and stamps the
    /// current UTC time (Unix epoch milliseconds) as the event timestamp.
    /// </summary>
    public DiagnosticEvent()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticEvent"/> class as a copy of an
    /// existing event, replacing the message. Used to produce a sanitized/masked copy without
    /// mutating the original event.
    /// </summary>
    /// <param name="originalEvent">The original diagnostic event to copy.</param>
    /// <param name="sanitizedMessage">The sanitized message to use instead of the original.</param>
    public DiagnosticEvent(DiagnosticEvent originalEvent, string sanitizedMessage)
    {
        // ArgumentNullException.ThrowIfNull isn't available on netstandard2.0 (added in .NET 6),
        // and this package multi-targets netstandard2.0;net8.0 — use the classic null-check form
        // instead so it compiles identically on both targets.
        if (originalEvent == null)
        {
            throw new ArgumentNullException(nameof(originalEvent));
        }

        sanitizedMessage ??= "#"; // fallback

        Timestamp = originalEvent.Timestamp;
        Type = originalEvent.Type;
        Message = sanitizedMessage;
        StackTrace = originalEvent.StackTrace;
        Data = originalEvent.Data;
        TraceLevel = originalEvent.TraceLevel;
    }

    /// <summary>Gets the timestamp of the event, in Unix epoch milliseconds.</summary>
    public long Timestamp { get; }

    /// <summary>Gets or sets the type of the diagnostic event.</summary>
    public DiagnosticEventType Type { get; set; }

    /// <summary>Gets or sets the message describing the event.</summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// Gets or sets the stack trace associated with the event, if available. Omitted from JSON
    /// serialization when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets additional data related to the event, if any. Omitted from JSON serialization
    /// when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets the trace level of the event, if applicable. Omitted from JSON serialization
    /// when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TraceLevel? TraceLevel { get; set; }
}
