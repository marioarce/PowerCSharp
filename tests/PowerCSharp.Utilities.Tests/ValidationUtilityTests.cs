using PowerCSharp.Utilities;
using Xunit;

namespace PowerCSharp.Utilities.Tests;

public class ValidationUtilityTests
{
    [Fact]
    public void IsValidEmail_ShouldReturnTrueForValidEmail()
    {
        // Arrange
        string email = "test@example.com";
        
        // Act
        bool result = ValidationUtility.IsValidEmail(email);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsValidEmail_ShouldReturnFalseForInvalidEmail()
    {
        // Arrange
        string email = "invalid-email";
        
        // Act
        bool result = ValidationUtility.IsValidEmail(email);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValidEmail_ShouldReturnFalseForNullEmail()
    {
        // Arrange
        string? email = null;
        
        // Act
        bool result = ValidationUtility.IsValidEmail(email);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValidEmail_ShouldReturnFalseForEmptyEmail()
    {
        // Arrange
        string email = "";
        
        // Act
        bool result = ValidationUtility.IsValidEmail(email);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValidEmail_ShouldReturnFalseForWhitespaceEmail()
    {
        // Arrange
        string email = "   ";
        
        // Act
        bool result = ValidationUtility.IsValidEmail(email);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsNumeric_ShouldReturnTrueForValidNumbers()
    {
        // Arrange
        string number = "12345";
        
        // Act
        bool result = ValidationUtility.IsNumeric(number);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsNumeric_ShouldReturnFalseForNonNumericString()
    {
        // Arrange
        string text = "abc123";
        
        // Act
        bool result = ValidationHelper.IsNumeric(text);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsNumeric_ShouldReturnFalseForNullString()
    {
        // Arrange
        string? value = null;
        
        // Act
        bool result = ValidationHelper.IsNumeric(value);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsNumeric_ShouldReturnFalseForEmptyString()
    {
        // Arrange
        string value = "";
        
        // Act
        bool result = ValidationHelper.IsNumeric(value);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValidUrl_ShouldReturnTrueForValidHttpUrl()
    {
        // Arrange
        string url = "http://example.com";
        
        // Act
        bool result = ValidationUtility.IsValidUrl(url);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsValidUrl_ShouldReturnTrueForValidHttpsUrl()
    {
        // Arrange
        string url = "https://example.com";
        
        // Act
        bool result = ValidationUtility.IsValidUrl(url);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsValidUrl_ShouldReturnFalseForInvalidUrl()
    {
        // Arrange
        string url = "not-a-url";
        
        // Act
        bool result = ValidationUtility.IsValidUrl(url);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValidUrl_ShouldReturnFalseForFtpUrl()
    {
        // Arrange
        string url = "ftp://example.com";
        
        // Act
        bool result = ValidationUtility.IsValidUrl(url);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValidUrl_ShouldReturnFalseForNullUrl()
    {
        // Arrange
        string? url = null;
        
        // Act
        bool result = ValidationUtility.IsValidUrl(url);
        
        // Assert
        Assert.False(result);
    }
}
