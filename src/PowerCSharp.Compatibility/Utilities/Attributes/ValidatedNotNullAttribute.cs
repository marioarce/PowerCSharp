using System;

namespace PowerCSharp.Compatibility.Utilities.Attributes;

/// <summary>
/// Used to indicate that a method parameter is validated for not null.
/// This is used by code analysis to suppress warnings about possible null argument passed to a method.
/// </summary>
/// <remarks>
/// This attribute should be used on parameters of methods that validate arguments,
/// so that callers of the method do not get warnings that the argument must be non-null.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ValidatedNotNullAttribute : Attribute
{
}
