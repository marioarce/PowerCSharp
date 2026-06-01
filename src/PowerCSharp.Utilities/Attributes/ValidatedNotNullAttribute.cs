using System;

namespace PowerCSharp.Utilities.Attributes;

//
// Summary:
//     Represents an attribute indicating that parameter is checked for null inside
//     method. It is used to suppress "CA1062: Validate arguments of public methods"
//     code analysis warning.
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ValidatedNotNullAttribute : Attribute
{
}
