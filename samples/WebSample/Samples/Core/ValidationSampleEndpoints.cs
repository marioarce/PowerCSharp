using PowerCSharp.Utilities;

namespace WebSample.Samples.Core;

/// <summary>
/// Sample endpoints demonstrating validation utilities
/// </summary>
public static class ValidationSampleEndpoints
{
    /// <summary>
    /// Gets validation demo data
    /// </summary>
    /// <returns>Demo results showing various validation operations</returns>
    public static object GetDemoData()
    {
        string email = "user@example.com";
        string phone = "12345";
        string url = "https://github.com/marioarce/PowerCSharp";
        
        return new
        {
            email = new { value = email, isValid = ValidationUtility.IsValidEmail(email) },
            phone = new { value = phone, isNumeric = ValidationUtility.IsNumeric(phone) },
            url = new { value = url, isValid = ValidationUtility.IsValidUrl(url) }
        };
    }
}
