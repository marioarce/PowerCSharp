namespace PowerCSharp.Operational;

/// <summary>
/// Well-known HTTP header names used to activate in-app diagnostics tooling for a single request.
/// A developer or QA engineer sets these headers on a request to turn on debug/verbose diagnostics,
/// disk event logging, or a specific trace level for troubleshooting, without redeploying or
/// touching configuration.
/// </summary>
public static class DiagnosticHeaders
{
    /// <summary>Header name that enables diagnostics for the request.</summary>
    public const string Debug = "debug";

    /// <summary>Header name that enables verbose diagnostics (disables obfuscation) for the request.</summary>
    public const string DebugVerbose = "debugVerbose";

    /// <summary>Header name that sets the minimum trace level for the request.</summary>
    public const string TraceLevel = "traceLevel";

    /// <summary>Header name that enables disk event-log writing for the request.</summary>
    public const string EventLog = "eventLog";

    /// <summary>Header name that disables caching for the request.</summary>
    public const string CacheDisabled = "cacheDisabled";

    /// <summary>Header name that enables performance profiling for the request.</summary>
    public const string Performance = "performance";

    /// <summary>The header value that means "enabled" for every header above.</summary>
    public const string EnabledValue = "true";
}
