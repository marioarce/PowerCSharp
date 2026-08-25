namespace PowerCSharp.Operational.Abstractions;

/// <summary>
/// Configuration options for PowerCSharp.Operational, bound from the <c>PowerFeatures:Operational</c>
/// configuration section (or supplied directly to <c>AddOperational()</c>).
/// </summary>
public sealed class OperationalOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Operational is enabled. When <c>false</c>, NoOp
    /// implementations are registered so the host application behaves exactly as if Operational
    /// were never referenced. Defaults to <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the base directory path where disk event-log files are written. Required for
    /// <c>EventLogWriter</c> to write anything; if unset, disk logging is inert.
    /// </summary>
    public string? LogsBasePath { get; set; }

    /// <summary>
    /// Gets or sets the number of days disk event-log files are retained before
    /// <c>EventLogRetentionCleaner</c> deletes them. Defaults to 30.
    /// </summary>
    public int LogsRetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the application name used to namespace disk event-log files. If unset, falls
    /// back to the entry assembly name at runtime.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// Gets or sets the default minimum <see cref="Microsoft.Extensions.Logging.LogLevel"/> applied
    /// when no per-request trace-level header is present. Defaults to <c>Warning</c>.
    /// </summary>
    public Microsoft.Extensions.Logging.LogLevel DefaultLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Warning;

    /// <summary>Gets or sets the default maximum retry attempts for outbound HTTP calls. Defaults to 2.</summary>
    public int DefaultHttpMaxAttempts { get; set; } = 2;

    /// <summary>Gets or sets the default maximum retry attempts for generic method-level retries. Defaults to 2.</summary>
    public int DefaultMethodMaxAttempts { get; set; } = 2;
}
