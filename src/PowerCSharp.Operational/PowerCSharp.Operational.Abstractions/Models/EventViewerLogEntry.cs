using Microsoft.Extensions.Logging;

namespace PowerCSharp.Operational.Abstractions.Models;

/// <summary>
/// Represents a single log entry destined for a platform event log (e.g. the Windows Event
/// Viewer), including timestamp, category, level, message, correlation id, and optional exception
/// text. Defined here — in the cross-platform Abstractions package — so the core
/// <c>PowerCSharp.Operational</c> package can depend on the shape without depending on any
/// platform-specific event log implementation.
/// </summary>
/// <remarks>
/// A plain class rather than a C# record — the package targets <c>netstandard2.0</c>, whose
/// reference assemblies don't include <c>System.Runtime.CompilerServices.IsExternalInit</c>, so a
/// record's compiler-generated <c>init</c> accessors fail to compile there with CS0518. Matches the
/// convention already used by
/// <c>PowerCSharp.Feature.Sanitization.Abstractions.SanitizationResult</c>. All parameter names are
/// preserved from the original record so existing named-argument call sites
/// (<c>new EventViewerLogEntry(Timestamp: ..., CorrelationId: ..., ...)</c>) keep compiling
/// unchanged.
/// </remarks>
public sealed class EventViewerLogEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventViewerLogEntry"/> class.
    /// </summary>
    /// <param name="Timestamp">The UTC timestamp the entry was created.</param>
    /// <param name="Category">The category or source of the log entry (typically the logger category name).</param>
    /// <param name="Level">The severity level of the entry.</param>
    /// <param name="Message">The already-sanitized log message.</param>
    /// <param name="CorrelationId">The correlation id associated with the entry, if any.</param>
    /// <param name="ExceptionText">The already-sanitized exception text, if any.</param>
    /// <param name="Tags">Optional key/value tags for additional metadata.</param>
    public EventViewerLogEntry(
        DateTime Timestamp,
        string Category,
        LogLevel Level,
        string Message,
        string? CorrelationId,
        string? ExceptionText,
        IDictionary<string, string>? Tags = null)
    {
        this.Timestamp = Timestamp;
        this.Category = Category;
        this.Level = Level;
        this.Message = Message;
        this.CorrelationId = CorrelationId;
        this.ExceptionText = ExceptionText;
        this.Tags = Tags;
    }

    /// <summary>Gets the UTC timestamp the entry was created.</summary>
    public DateTime Timestamp { get; }

    /// <summary>Gets the category or source of the log entry (typically the logger category name).</summary>
    public string Category { get; }

    /// <summary>Gets the severity level of the entry.</summary>
    public LogLevel Level { get; }

    /// <summary>Gets the already-sanitized log message.</summary>
    public string Message { get; }

    /// <summary>Gets the correlation id associated with the entry, if any.</summary>
    public string? CorrelationId { get; }

    /// <summary>Gets the already-sanitized exception text, if any.</summary>
    public string? ExceptionText { get; }

    /// <summary>Gets optional key/value tags for additional metadata.</summary>
    public IDictionary<string, string>? Tags { get; }
}
