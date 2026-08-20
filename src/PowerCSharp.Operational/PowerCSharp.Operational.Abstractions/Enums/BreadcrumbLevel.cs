namespace PowerCSharp.Operational.Abstractions.Enums;

/// <summary>
/// Specifies the severity level of a breadcrumb — a recorded checkpoint in application flow used
/// to reconstruct the sequence of events leading up to an error or issue.
/// </summary>
public enum BreadcrumbLevel
{
    /// <summary>Verbose diagnostic detail.</summary>
    Debug = 0,

    /// <summary>General informational checkpoint. The default level.</summary>
    Info = 1,

    /// <summary>A noteworthy but non-error checkpoint.</summary>
    Warning = 2,

    /// <summary>An error-level checkpoint.</summary>
    Error = 3,

    /// <summary>A critical/fatal-level checkpoint.</summary>
    Critical = 4
}
