using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.Enums;
using PowerCSharp.Operational.Abstractions.Models;
using PowerCSharp.Operational.EventLog;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using TraceLevel = PowerCSharp.Operational.Abstractions.Enums.TraceLevel;

namespace PowerCSharp.Operational;

/// <summary>
/// Provides diagnostics and tracing functionality for a unit of work (typically an HTTP request),
/// including event capture, error tracking, and sensitive-data obfuscation. Manages diagnostic
/// events in a thread-safe manner and supports filtering, masking, and forwarding to disk (via
/// <see cref="EventLogWriter"/>).
/// </summary>
public sealed class DiagnosticsService : IDiagnosticsService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly OperationalOptions _options;
    private readonly EventLogWriter _eventLogWriter;

    /* DevNote: ConcurrentBag is used deliberately here to keep the event collection thread-safe.
     * Prefer ConcurrentBag<T>/ConcurrentQueue<T>/ConcurrentDictionary<TKey,TValue> over List<T>
     * for any collection touched from more than one logical flow of a single request. */
    private readonly ConcurrentBag<DiagnosticEvent> _events = new();

    private bool _initialized;
    private bool _enabled;
    private bool _verbose;
    private int _traceLevel;
    private bool _eventLog;
    private bool _cacheDisabled;
    private bool _performanceEnabled;

    private const char DefaultMaskChar = '*';
    private const TraceLevel DefaultTraceLevel = Abstractions.Enums.TraceLevel.Error;

    private static readonly Regex _ipRegex = new(@"\b\d{1,3}(\.\d{1,3}){3}\b", RegexOptions.Compiled);
    private static readonly Regex _urlRegex = new(@"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _emailRegex = new(@"\b[\w\.-]+@[\w\.-]+\.\w+\b", RegexOptions.Compiled);
    private static readonly Regex _guidRegex = new(@"\b[a-fA-F0-9]{8}\b(-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12}\b", RegexOptions.Compiled);

    /// <summary>
    /// Gets the default minimum <see cref="LogLevel"/> applied by <c>DiagnosticsLogger</c> when no
    /// per-request trace-level header is present. Cached here from <see cref="OperationalOptions"/>
    /// at construction so classes without direct access to the options (e.g. logger instances
    /// created by <c>ILoggerProvider</c> outside the DI scope) can still read it.
    /// </summary>
    public static LogLevel DefaultLogLevel { get; private set; } = LogLevel.Warning;

    /// <summary>Gets the default maximum retry attempts for outbound HTTP calls. See <see cref="DefaultLogLevel"/> remarks.</summary>
    public static int DefaultHttpMaxAttempts { get; private set; } = 2;

    /// <summary>Gets the default maximum retry attempts for generic method-level retries. See <see cref="DefaultLogLevel"/> remarks.</summary>
    public static int DefaultMethodMaxAttempts { get; private set; } = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="options">The Operational configuration options.</param>
    /// <param name="eventLogWriter">The disk event-log writer diagnostic events are forwarded to.</param>
    public DiagnosticsService(IHttpContextAccessor httpContextAccessor, IOptions<OperationalOptions> options, EventLogWriter eventLogWriter)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(eventLogWriter);

        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
        _eventLogWriter = eventLogWriter;

        DefaultLogLevel = _options.DefaultLogLevel;
        DefaultHttpMaxAttempts = _options.DefaultHttpMaxAttempts;
        DefaultMethodMaxAttempts = _options.DefaultMethodMaxAttempts;

        Initialize();
    }

    /// <inheritdoc />
    public bool IsEnabled => _enabled;

    /// <inheritdoc />
    public bool IsVerbose => _verbose;

    /// <inheritdoc />
    public int TraceLevel => _traceLevel;

    /// <inheritdoc />
    public bool IsEventLogEnabled => _eventLog;

    /// <inheritdoc />
    public bool IsCacheDisabled => _cacheDisabled;

    /// <inheritdoc />
    public bool IsDebugVerbose => _enabled && _verbose;

    /// <inheritdoc />
    public bool IsPerformanceEnabled => _enabled && _performanceEnabled;

    /// <inheritdoc />
    public List<DiagnosticEvent>? GetEvents() => GetFilteredListOfEvents();

    /// <inheritdoc />
    public DiagnosticEvent? AddTrace(string message, TraceLevel level = Abstractions.Enums.TraceLevel.Error, object? data = null, bool obfuscateMessage = false)
    {
        if (!_enabled)
        {
            return null;
        }

        if (obfuscateMessage || ShouldAutoObfuscate(message))
        {
            message = MaskString(message);
        }

        var result = new DiagnosticEvent
        {
            Type = DiagnosticEventType.Trace,
            Message = message,
            Data = Obfuscate(data),
            TraceLevel = level
        };

        _events.Add(result);
        TryLogToFile(result);

        return result;
    }

    /// <inheritdoc />
    public DiagnosticEvent? AddBreadcrumb(string message, string category, BreadcrumbLevel level = BreadcrumbLevel.Info, bool obfuscateMessage = false)
    {
        if (!_enabled)
        {
            return null;
        }

        var fullMessage = $"{category}: {message}";

        if (obfuscateMessage || ShouldAutoObfuscate(message))
        {
            fullMessage = MaskString(fullMessage);
        }

        var result = new DiagnosticEvent
        {
            Type = DiagnosticEventType.Breadcrumb,
            Message = fullMessage,
            Data = null
        };

        _events.Add(result);
        TryLogToFile(result);

        return result;
    }

    /// <inheritdoc />
    public DiagnosticEvent? AddException(Exception ex, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        if (!_enabled)
        {
            return null;
        }

        var result = new DiagnosticEvent
        {
            Type = DiagnosticEventType.Exception,
            Message = MaskString(ex.ToString()),
            StackTrace = ex.StackTrace,
            Data = Obfuscate(data)
        };

        _events.Add(result);
        TryLogToFile(result);

        return result;
    }

    /// <inheritdoc />
    public DiagnosticEvent? AddError(string message, object? data = null, bool obfuscateMessage = false)
    {
        if (!_enabled)
        {
            return null;
        }

        if (obfuscateMessage || ShouldAutoObfuscate(message))
        {
            message = MaskString(message);
        }

        var result = new DiagnosticEvent
        {
            Type = DiagnosticEventType.Error,
            Message = message,
            Data = Obfuscate(data)
        };

        _events.Add(result);
        TryLogToFile(result);

        return result;
    }

    /// <inheritdoc />
    public DiagnosticsPayload? BuildPayload()
    {
        if (!_enabled)
        {
            return null;
        }

        return new DiagnosticsPayload
        {
            Events = GetFilteredListOfEvents()
        };
    }

    /// <inheritdoc />
    public object? Obfuscate(object? input)
    {
        if (input == null || IsDebugVerbose)
        {
            return input;
        }

        return ObfuscateInternal(input);
    }

    /// <summary>
    /// Initializes diagnostics settings from the current HTTP context's request headers. Runs once
    /// per instance (this service is registered scoped/per-request).
    /// </summary>
    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        var headers = _httpContextAccessor.HttpContext?.Request?.Headers;

        if (headers != null)
        {
            _enabled = headers.ContainsKey(DiagnosticHeaders.Debug) && headers[DiagnosticHeaders.Debug] == DiagnosticHeaders.EnabledValue;
            _verbose = headers.ContainsKey(DiagnosticHeaders.DebugVerbose) && headers[DiagnosticHeaders.DebugVerbose] == DiagnosticHeaders.EnabledValue;

            if (headers.TryGetValue(DiagnosticHeaders.TraceLevel, out var value))
            {
                var traceLevelHeader = value.FirstOrDefault();
                _traceLevel = int.TryParse(traceLevelHeader, out var level) ? level : Convert.ToInt16(DefaultTraceLevel);
            }
            else
            {
                _traceLevel = Convert.ToInt16(DefaultTraceLevel);
            }

            _eventLog = headers.ContainsKey(DiagnosticHeaders.EventLog) && headers[DiagnosticHeaders.EventLog] == DiagnosticHeaders.EnabledValue;
            _cacheDisabled = headers.ContainsKey(DiagnosticHeaders.CacheDisabled) && headers[DiagnosticHeaders.CacheDisabled] == DiagnosticHeaders.EnabledValue;
            _performanceEnabled = headers.ContainsKey(DiagnosticHeaders.Performance) && headers[DiagnosticHeaders.Performance] == DiagnosticHeaders.EnabledValue;
        }
        else
        {
            _enabled = false;
            _verbose = false;
            _traceLevel = Convert.ToInt16(DefaultTraceLevel);
            _eventLog = false;
            _cacheDisabled = false;
            _performanceEnabled = false;
        }

        _initialized = true;
    }

    /// <summary>
    /// Masks a string using the shared default mask character. Delegates to
    /// <c>PowerCSharp.Feature.Sanitization.Abstractions</c> so Operational and Sanitization apply
    /// exactly the same masking algorithm.
    /// </summary>
    private static string MaskString(string value) => value.Mask(DefaultMaskChar);

    /// <summary>
    /// Obfuscates sensitive data within the provided object using reflection, honoring
    /// <see cref="Abstractions.Models.SensitiveDataAttribute"/> on individual properties.
    /// </summary>
    private object ObfuscateInternal(object input)
    {
        try
        {
            if (input is string str)
            {
                return MaskString(str);
            }

            if (input is System.Collections.Generic.IDictionary<string, object> dictionary)
            {
                var maskedDict = new Dictionary<string, object>();
                foreach (var kvp in dictionary)
                {
                    maskedDict[kvp.Key] = ObfuscateInternal(kvp.Value);
                }
                return maskedDict;
            }

            var type = input.GetType();
            var resultDict = new Dictionary<string, object?>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                var value = property.GetValue(input);

                if (value is string s)
                {
                    var attribute = property.GetCustomAttribute<Abstractions.Models.SensitiveDataAttribute>();
                    resultDict[property.Name] = attribute != null
                        ? s.Mask(attribute.Length, attribute.MaskChar)
                        : MaskString(s);
                }
                else if (value is System.Collections.Generic.IDictionary<string, object> nestedDict)
                {
                    resultDict[property.Name] = ObfuscateInternal(nestedDict);
                }
                else if (value != null && !IsSimpleType(value.GetType()))
                {
                    resultDict[property.Name] = ObfuscateInternal(value);
                }
                else
                {
                    resultDict[property.Name] = value;
                }
            }

            return resultDict;
        }
        catch
        {
            // Fail-safe: never let obfuscation failure surface as an exception. Return the
            // original, unobfuscated input rather than lose the diagnostic event entirely.
            return input;
        }
    }

    /// <summary>
    /// Determines whether a message likely contains an IP address, URL, email address, or GUID and
    /// should therefore be auto-obfuscated even when the caller didn't request it explicitly.
    /// </summary>
    private static bool ShouldAutoObfuscate(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        return _ipRegex.IsMatch(message)
            || _urlRegex.IsMatch(message)
            || _emailRegex.IsMatch(message)
            || _guidRegex.IsMatch(message);
    }

    /// <summary>
    /// Returns a trace-level-filtered, sensitive-data-sanitized snapshot of the recorded events.
    /// Sanitization is delegated to <c>PowerCSharp.Feature.Sanitization.Abstractions</c> — no local
    /// sensitive-data engine is reimplemented here.
    /// </summary>
    private List<DiagnosticEvent>? GetFilteredListOfEvents()
    {
        if (!_enabled)
        {
            return null;
        }

        var hasErrors = _events.Any(e => e.Type is DiagnosticEventType.Error or DiagnosticEventType.Exception);

        var traceLevel = this.TraceLevel;
        if (hasErrors)
        {
            // If an error/exception was captured, force full trace output for troubleshooting.
            traceLevel = Convert.ToInt16(Abstractions.Enums.TraceLevel.Trace);
        }

        if (traceLevel == Convert.ToInt16(Abstractions.Enums.TraceLevel.None))
        {
            traceLevel = int.MaxValue;
        }

        if (IsDebugVerbose)
        {
            traceLevel = int.MinValue;
        }

        return _events
            .Where(e => Convert.ToInt16(e.TraceLevel) >= traceLevel)
            .OrderBy(e => e.Timestamp)
            .Select(e => new DiagnosticEvent(e, e.Message.SanitizeForSensitiveData()))
            .ToList();
    }

    /// <summary>
    /// Forwards a diagnostic event to disk via <see cref="EventLogWriter"/>, if disk event logging
    /// is enabled for the current request and a base path is configured.
    /// </summary>
    private void TryLogToFile(DiagnosticEvent? data)
    {
        if (!IsEventLogEnabled || data == null || string.IsNullOrEmpty(_options.LogsBasePath))
        {
            return;
        }

        _eventLogWriter.Enqueue(data);
    }

    /// <summary>Determines whether the specified type is a simple, non-decomposable type.</summary>
    private static bool IsSimpleType(Type type) =>
        type.IsPrimitive
        || type.IsEnum
        || type == typeof(string)
        || type == typeof(decimal)
        || type == typeof(DateTime)
        || type == typeof(Guid)
        || type == typeof(DateTimeOffset)
        || type == typeof(TimeSpan);
}
