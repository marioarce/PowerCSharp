using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.Models;
using PowerCSharp.Operational.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace PowerCSharp.Operational.EventLog;

/// <summary>
/// Writes diagnostic events to disk as NDJSON, organized by application name, date, and
/// correlation id. Serialization and file I/O happen on a dedicated background task so callers
/// (e.g. <see cref="DiagnosticsService"/>) never block on disk writes.
/// </summary>
/// <remarks>
/// Registered as a DI singleton (<c>services.AddSingleton&lt;EventLogWriter&gt;()</c>) rather than a
/// hand-rolled static <c>Instance</c> — this is a deliberate improvement over the original
/// implementation's static-locator pattern, since every dependency here
/// (<see cref="IHttpContextAccessor"/>, <see cref="IEventViewerService"/>, <see cref="OperationalOptions"/>)
/// is itself singleton-safe and can be constructor-injected cleanly.
/// </remarks>
public sealed class EventLogWriter : IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventViewerService _eventViewerService;
    private readonly ILogger<EventLogWriter> _logger;
    private readonly string? _appName;
    private readonly string? _basePath;

    private readonly BlockingCollection<Logging.LogWriteRequest> _logQueue = new(new ConcurrentQueue<Logging.LogWriteRequest>());
    private readonly ConcurrentDictionary<string, object> _fileLocks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLogWriter"/> class and starts the
    /// background queue-processing task.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context (used to resolve a correlation id).</param>
    /// <param name="eventViewerService">The event-viewer forwarding hook, used only to report writer failures.</param>
    /// <param name="options">The Operational configuration options.</param>
    /// <param name="logger">The logger used to record writer failures.</param>
    public EventLogWriter(
        IHttpContextAccessor httpContextAccessor,
        IEventViewerService eventViewerService,
        IOptions<OperationalOptions> options,
        ILogger<EventLogWriter> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpContextAccessor = httpContextAccessor;
        _eventViewerService = eventViewerService;
        _logger = logger;

        _appName = options.Value.AppName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "App";
        _basePath = options.Value.LogsBasePath;

        try
        {
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to start EventLogWriter background queue-processing task.");
        }
    }

    /// <summary>
    /// Enqueues a diagnostic event for asynchronous writing to disk. Organizes files by
    /// application, date, and correlation id, serialized as a single NDJSON line.
    /// </summary>
    /// <param name="data">The object to serialize and write.</param>
    public void Enqueue(object data)
    {
        try
        {
            if (string.IsNullOrEmpty(_basePath))
            {
                return; // Disk logging is inert until LogsBasePath is configured.
            }

            var httpContext = _httpContextAccessor.HttpContext;

            // TODO: resolve the request/operation correlation id here once PowerCSharp ships a
            // correlation-id abstraction. Until then, fall back to a fresh id per log line so
            // files stay uniquely named (flagged in the architecture plan as a known v1 gap).
            var correlationId = httpContext?.TraceIdentifier;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            correlationId = SanitizeForFileSystem(correlationId);

            var now = DateTime.UtcNow;
            var folder = Path.Combine(_basePath, _appName!, $"{now.Year}", $"{now.Month:D2}", $"{now.Day:D2}");
            Directory.CreateDirectory(folder);

            var fileName = $"log_{now.Year}_{now.Month:D2}_{now.Day:D2}_{now.Hour:D2}_{now.Minute:D2}_{correlationId}.ndjson";
            var filePath = Path.Combine(folder, fileName);

            var content = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logQueue.Add(new Logging.LogWriteRequest(filePath, content));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue an event-log entry.");
        }
    }

    /// <summary>Processes the queue and writes each entry to disk, one file lock at a time per path.</summary>
    private void ProcessQueue()
    {
        try
        {
            foreach (var request in _logQueue.GetConsumingEnumerable())
            {
                try
                {
                    var fileLock = _fileLocks.GetOrAdd(request.FilePath, _ => new object());

                    lock (fileLock)
                    {
                        using var stream = new FileStream(request.FilePath, FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: false);
                        var bytes = Encoding.UTF8.GetBytes(request.Content + Environment.NewLine);

                        // Synchronous write+flush deliberately: avoids async/disposal race
                        // conditions on a dedicated long-running background thread.
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write event-log entry to '{FilePath}'.", request.FilePath);
                    TryForwardFailureToEventViewer(request.FilePath, ex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "EventLogWriter background queue-processing task failed.");
        }
    }

    /// <summary>Strips characters that are unsafe in a file/directory name.</summary>
    private static string SanitizeForFileSystem(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Where(c => !invalid.Contains(c)).ToArray());
    }

    /// <summary>
    /// Best-effort fallback: when a disk write fails, forward the failure through
    /// <see cref="IEventViewerService"/> so it isn't only visible in the app's own logs (which may
    /// themselves depend on disk availability). A no-op when only <c>NoOpEventViewerService</c> is
    /// registered.
    /// </summary>
    private void TryForwardFailureToEventViewer(string filePath, Exception ex)
    {
        try
        {
            _eventViewerService.TryEnqueue(new EventViewerLogEntry(
                DateTime.UtcNow,
                nameof(EventLogWriter),
                LogLevel.Error,
                $"Failed to write event-log entry to '{filePath}'.",
                CorrelationId: null,
                ExceptionText: ex.ToString()));
        }
        catch
        {
            // Forwarding is best-effort only; never let a secondary failure mask the original one.
        }
    }

    /// <inheritdoc />
    public void Dispose() => _logQueue.CompleteAdding();
}
