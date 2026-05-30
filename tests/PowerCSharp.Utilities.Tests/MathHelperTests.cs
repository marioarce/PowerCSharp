using PowerCSharp.Utilities;
using Xunit;

namespace PowerCSharp.Utilities.Tests;

public class MathHelperTests
{
    [Fact]
    public void Clamp_ShouldReturnMinWhenValueIsBelowMin()
    {
        // Arrange
        int value = 5;
        int min = 10;
        int max = 20;
        
        // Act
        int result = MathHelper.Clamp(value, min, max);
        
        // Assert
        Assert.Equal(min, result);
    }
    
    [Fact]
    public void Clamp_ShouldReturnMaxWhenValueIsAboveMax()
    {
        // Arrange
        int value = 25;
        int min = 10;
        int max = 20;
        
        // Act
        int result = MathHelper.Clamp(value, min, max);
        
        // Assert
        Assert.Equal(max, result);
    }
    
    [Fact]
    public void Clamp_ShouldReturnValueWhenInRange()
    {
        // Arrange
        int value = 15;
        int min = 10;
        int max = 20;
        
        // Act
        int result = MathHelper.Clamp(value, min, max);
        
        // Assert
        Assert.Equal(value, result);
    }
    
    [Fact]
    public void IsInRange_ShouldReturnTrueWhenValueInRange()
    {
        // Arrange
        int value = 15;
        int min = 10;
        int max = 20;
        
        // Act
        bool result = MathHelper.IsInRange(value, min, max);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsInRange_ShouldReturnTrueWhenValueEqualsMin()
    {
        // Arrange
        int value = 10;
        int min = 10;
        int max = 20;
        
        // Act
        bool result = MathHelper.IsInRange(value, min, max);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsInRange_ShouldReturnTrueWhenValueEqualsMax()
    {
        // Arrange
        int value = 20;
        int min = 10;
        int max = 20;
        
        // Act
        bool result = MathHelper.IsInRange(value, min, max);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsInRange_ShouldReturnFalseWhenValueBelowMin()
    {
        // Arrange
        int value = 5;
        int min = 10;
        int max = 20;
        
        // Act
        bool result = MathHelper.IsInRange(value, min, max);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsInRange_ShouldReturnFalseWhenValueAboveMax()
    {
        // Arrange
        int value = 25;
        int min = 10;
        int max = 20;
        
        // Act
        bool result = MathHelper.IsInRange(value, min, max);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Percentage_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        double part = 25;
        double total = 100;
        
        // Act
        double result = MathHelper.Percentage(part, total);
        
        // Assert
        Assert.Equal(25.0, result);
    }
    
    [Fact]
    public void Percentage_ShouldReturnZeroWhenTotalIsZero()
    {
        // Arrange
        double part = 25;
        double total = 0;
        
        // Act
        double result = MathHelper.Percentage(part, total);
        
        // Assert
        Assert.Equal(0.0, result);
    }
    
    [Fact]
    public void ToRadians_ShouldConvertDegreesToRadians()
    {
        // Arrange
        double degrees = 180;
        
        // Act
        double result = MathHelper.ToRadians(degrees);
        
        // Assert
        Assert.Equal(Math.PI, result, 5);
    }
    
    [Fact]
    public void ToDegrees_ShouldConvertRadiansToDegrees()
    {
        // Arrange
        double radians = Math.PI;
        
        // Act
        double result = MathHelper.ToDegrees(radians);
        
        // Assert
        Assert.Equal(180.0, result, 5);
    }
    
    [Fact]
    public void IsEven_ShouldReturnTrueForEvenNumber()
    {
        // Arrange
        int number = 4;
        
        // Act
        bool result = MathHelper.IsEven(number);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsEven_ShouldReturnFalseForOddNumber()
    {
        // Arrange
        int number = 3;
        
        // Act
        bool result = MathHelper.IsEven(number);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsOdd_ShouldReturnTrueForOddNumber()
    {
        // Arrange
        int number = 3;
        
        // Act
        bool result = MathHelper.IsOdd(number);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsOdd_ShouldReturnFalseForEvenNumber()
    {
        // Arrange
        int number = 4;
        
        // Act
        bool result = MathHelper.IsOdd(number);
        
        // Assert
        Assert.False(result);
    }
}
