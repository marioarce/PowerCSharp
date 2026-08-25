namespace PowerCSharp.Operational.Logging;

/// <summary>
/// A no-operation disposable logging scope, returned by <see cref="DiagnosticsLogger.BeginScope{TState}"/>
/// when a scope is required by the <c>ILogger</c> contract but no actual resource management is needed.
/// </summary>
public sealed class NullScope : IDisposable
{
    /// <summary>Gets the singleton instance of <see cref="NullScope"/>.</summary>
    public static readonly NullScope Instance = new();

    private NullScope() { }

    /// <summary>Does nothing.</summary>
    public void Dispose() { }
}
