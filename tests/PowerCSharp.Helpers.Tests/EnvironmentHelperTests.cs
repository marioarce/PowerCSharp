using PowerCSharp.Helpers;
using Xunit;

namespace PowerCSharp.Helpers.Tests;

public class EnvironmentHelperTests
{
    [Fact]
    public void GetEnvironmentVariable_ShouldReturnValueWhenExists()
    {
        // Arrange
        string varName = "TEST_VAR";
        string expectedValue = "test_value";
        Environment.SetEnvironmentVariable(varName, expectedValue);
        
        // Act
        string result = EnvironmentHelper.GetEnvironmentVariable(varName);
        
        // Assert
        Assert.Equal(expectedValue, result);
        
        // Cleanup
        Environment.SetEnvironmentVariable(varName, null);
    }
    
    [Fact]
    public void GetEnvironmentVariable_ShouldReturnDefaultValueWhenNotExists()
    {
        // Arrange
        string varName = "NON_EXISTENT_VAR";
        string defaultValue = "default_value";
        
        // Act
        string result = EnvironmentHelper.GetEnvironmentVariable(varName, defaultValue);
        
        // Assert
        Assert.Equal(defaultValue, result);
    }
    
    [Fact]
    public void GetEnvironmentVariable_ShouldReturnEmptyStringWhenNotExistsAndNoDefault()
    {
        // Arrange
        string varName = "NON_EXISTENT_VAR";
        
        // Act
        string result = EnvironmentHelper.GetEnvironmentVariable(varName);
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void GetEnvironmentVariable_ShouldReturnEmptyStringWhenVarExistsButEmpty()
    {
        // Arrange
        string varName = "EMPTY_VAR";
        Environment.SetEnvironmentVariable(varName, "");
        
        // Act
        string result = EnvironmentHelper.GetEnvironmentVariable(varName);
        
        // Assert
        Assert.Equal("", result);
        
        // Cleanup
        Environment.SetEnvironmentVariable(varName, null);
    }
    
    [Fact]
    public void IsDevelopment_ShouldReturnTrueWhenEnvironmentIsDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        
        // Act
        bool result = EnvironmentHelper.IsDevelopment();
        
        // Assert
        Assert.True(result);
        
        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
    
    [Fact]
    public void IsDevelopment_ShouldReturnTrueWhenDotNetEnvironmentIsDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        
        // Act
        bool result = EnvironmentHelper.IsDevelopment();
        
        // Assert
        Assert.True(result);
        
        // Cleanup
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
    }
    
    [Fact]
    public void IsDevelopment_ShouldReturnFalseWhenEnvironmentIsProduction()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        
        // Act
        bool result = EnvironmentHelper.IsDevelopment();
        
        // Assert
        Assert.False(result);
        
        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
    
    [Fact]
    public void IsDevelopment_ShouldReturnFalseWhenNoEnvironmentSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        
        // Act
        bool result = EnvironmentHelper.IsDevelopment();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsDevelopment_ShouldReturnFalseWhenEnvironmentIsStaging()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging");
        
        // Act
        bool result = EnvironmentHelper.IsDevelopment();
        
        // Assert
        Assert.False(result);
        
        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }
    
    [Fact]
    public void GetSafeMachineName_ShouldReturnValidMachineName()
    {
        // Act
        string result = EnvironmentHelper.GetSafeMachineName();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.DoesNotContain(" ", result);
        Assert.DoesNotContain("/", result);
        Assert.DoesNotContain("\\", result);
    }
    
    [Fact]
    public void GetApplicationVersion_ShouldReturnValidVersion()
    {
        // Act
        string result = EnvironmentHelper.GetApplicationVersion();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(".", result);
    }
    
    [Fact]
    public void GetApplicationVersion_ShouldReturnVersionInCorrectFormat()
    {
        // Act
        string result = EnvironmentHelper.GetApplicationVersion();
        
        // Assert
        Assert.Matches(@"^\d+\.\d+\.\d+\.\d+$", result);
    }
}
