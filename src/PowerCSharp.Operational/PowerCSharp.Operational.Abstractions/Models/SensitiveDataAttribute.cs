namespace PowerCSharp.Operational.Abstractions.Models;

/// <summary>
/// Marks a property as containing sensitive data that should be masked when the object is captured
/// in a diagnostic event. Applied by hosts to their own POCOs; <c>DiagnosticsService</c> reads it
/// via reflection when obfuscating captured data.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SensitiveDataAttribute : Attribute
{
    private const char DefaultMaskChar = '*';
    private const int DefaultVisibleChars = 10;

    /// <summary>Gets the number of characters to leave visible when masking.</summary>
    public int Length { get; }

    /// <summary>Gets the character used to mask the value.</summary>
    public char MaskChar { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveDataAttribute"/> class.
    /// </summary>
    /// <param name="length">The number of visible characters to leave unmasked. Defaults to 10.</param>
    /// <param name="maskChar">The character used for masking. Defaults to <c>*</c>.</param>
    public SensitiveDataAttribute(int length = DefaultVisibleChars, char maskChar = DefaultMaskChar)
    {
        Length = length;
        MaskChar = maskChar;
    }
}
