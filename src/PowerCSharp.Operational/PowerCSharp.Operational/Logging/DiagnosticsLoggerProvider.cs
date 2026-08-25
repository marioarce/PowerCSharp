using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerCSharp.Operational.Abstractions;
using System.Collections.Concurrent;

namespace PowerCSharp.Operational.Logging;

/// <summary>
/// Creates and manages <see cref="DiagnosticsLogger"/> instances, one per category name, for
/// registration with the <c>Microsoft.Extensions.Logging</c> infrastructure via
/// <c>ILoggingBuilder.AddProvider</c>.
/// </summary>
public sealed class DiagnosticsLoggerProvider : ILoggerProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventViewerService _eventViewerService;
    private readonly ConcurrentDictionary<string, DiagnosticsLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsLoggerProvider"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    /// <param name="eventViewerService">The event-viewer forwarding hook (NoOp by default).</param>
    public DiagnosticsLoggerProvider(IHttpContextAccessor httpContextAccessor, IEventViewerService eventViewerService)
    {
        _httpContextAccessor = httpContextAccessor;
        _eventViewerService = eventViewerService;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new DiagnosticsLogger(_httpContextAccessor, _eventViewerService, name));

    /// <inheritdoc />
    public void Dispose() => _loggers.Clear();
}
