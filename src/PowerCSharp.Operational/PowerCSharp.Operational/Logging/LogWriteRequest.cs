namespace PowerCSharp.Operational.Logging;

/// <summary>
/// Represents a queued request to write serialized log content to a specific file path. Used
/// internally by <see cref="EventLog.EventLogWriter"/> to hand work to its background writer task.
/// </summary>
public sealed class LogWriteRequest
{
    /// <summary>Gets the full file path to write to.</summary>
    public string FilePath { get; }

    /// <summary>Gets the serialized content to append to the file.</summary>
    public string Content { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogWriteRequest"/> class.
    /// </summary>
    /// <param name="filePath">The full file path to write to.</param>
    /// <param name="content">The serialized content to append.</param>
    public LogWriteRequest(string filePath, string content)
    {
        FilePath = filePath;
        Content = content;
    }
}
