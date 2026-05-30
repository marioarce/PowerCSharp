namespace PowerCSharp.Extensions.Configuration.Interfaces;

/// <summary>
/// Represents the interface for application options.
/// </summary>
public interface IAppOptions
{
    /// <summary>
    /// The configuration section path.
    /// </summary>
    string ConfigSectionPath { get; }
}
