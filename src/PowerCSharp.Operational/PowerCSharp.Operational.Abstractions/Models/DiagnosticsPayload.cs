using System.Text.Json.Serialization;

namespace PowerCSharp.Operational.Abstractions.Models;

/// <summary>
/// Represents a diagnostics payload — a snapshot collection of diagnostic events — returned to a
/// caller for troubleshooting purposes (for example, enriching an API response payload).
/// </summary>
public class DiagnosticsPayload
{
    /// <summary>
    /// Gets or sets the list of diagnostic events in this payload. Omitted from JSON serialization
    /// when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<DiagnosticEvent>? Events { get; set; }
}
