using PowerCSharp.Operational.Abstractions.Models;

namespace PowerCSharp.Operational.Abstractions.NoOp;

/// <summary>
/// Inert <see cref="IEventViewerService"/> registered by default. Every enqueue is a no-op that
/// reports success without doing anything, since there is nothing downstream to overflow — a
/// future <c>PowerCSharp.Operational.WinEventLog</c> provider package registers the real
/// implementation to override this floor on Windows hosts.
/// </summary>
public sealed class NoOpEventViewerService : IEventViewerService
{
    /// <inheritdoc />
    public bool TryEnqueue(EventViewerLogEntry entry) => true;
}
