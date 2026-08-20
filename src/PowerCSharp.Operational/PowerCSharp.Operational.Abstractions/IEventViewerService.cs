using PowerCSharp.Operational.Abstractions.Models;

namespace PowerCSharp.Operational.Abstractions;

/// <summary>
/// Defines a pluggable hook for forwarding log entries to a platform event log (e.g. the Windows
/// Event Viewer). The core <c>PowerCSharp.Operational</c> package ships only
/// <c>NoOpEventViewerService</c> against this contract — no platform-specific event log code lives
/// in the cross-platform core. A future <c>PowerCSharp.Operational.WinEventLog</c> provider package
/// supplies the real Windows implementation.
/// </summary>
public interface IEventViewerService
{
    /// <summary>
    /// Attempts to enqueue a log entry for forwarding, in a non-blocking manner.
    /// </summary>
    /// <param name="entry">The log entry to enqueue.</param>
    /// <returns><c>true</c> if the entry was enqueued; <c>false</c> if the queue was full or forwarding is inert (NoOp).</returns>
    bool TryEnqueue(EventViewerLogEntry entry);
}
