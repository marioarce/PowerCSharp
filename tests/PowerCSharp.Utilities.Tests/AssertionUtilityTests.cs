using PowerCSharp.Utilities;
using Xunit;

namespace PowerCSharp.Utilities.Tests;

public class AssertionUtilityTests
{
    [Fact]
    public void ArgumentNotNull_ShouldNotThrowWhenArgumentIsNotNull()
    {
        // Arrange
        object argument = new object();
        string argumentName = "test";
        
        // Act & Assert - Should not throw
        AssertionUtility.ArgumentNotNull(argument, argumentName);
    }

    [Fact]
    public void ArgumentNotNull_ShouldThrowWhenArgumentIsNull()
    {
        // Arrange
        object argument = null;
        string argumentName = "test";
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AssertionUtility.ArgumentNotNull(argument, argumentName));
    }

    [Fact]
    public void ArgumentNotNullOrEmpty_ShouldNotThrowWhenArgumentIsNotNullOrEmpty()
    {
        // Arrange
        string argument = "test";
        string argumentName = "test";
        
        // Act & Assert - Should not throw
        AssertionUtility.ArgumentNotNullOrEmpty(argument, argumentName);
    }

    [Fact]
    public void ArgumentNotNullOrEmpty_ShouldThrowWhenArgumentIsNull()
    {
        // Arrange
        string argument = null;
        string argumentName = "test";
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AssertionUtility.ArgumentNotNullOrEmpty(argument, argumentName));
    }

    [Fact]
    public void ArgumentNotNullOrEmpty_ShouldThrowWhenArgumentIsEmpty()
    {
        // Arrange
        string argument = "";
        string argumentName = "test";
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => AssertionUtility.ArgumentNotNullOrEmpty(argument, argumentName));
    }

    [Fact]
    public void IsTrue_ShouldNotThrowWhenConditionIsTrue()
    {
        // Arrange
        bool condition = true;
        string message = "Condition should be true";
        
        // Act & Assert - Should not throw
        AssertionUtility.IsTrue(condition, message);
    }

    [Fact]
    public void IsTrue_ShouldThrowWhenConditionIsFalse()
    {
        // Arrange
        bool condition = false;
        string message = "Condition should be true";
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => AssertionUtility.IsTrue(condition, message));
    }

    [Fact]
    public void IsFalse_ShouldNotThrowWhenConditionIsFalse()
    {
        // Arrange
        bool condition = false;
        string message = "Condition should be false";
        
        // Act & Assert - Should not throw
        AssertionUtility.IsFalse(condition, message);
    }

    [Fact]
    public void IsFalse_ShouldThrowWhenConditionIsTrue()
    {
        // Arrange
        bool condition = true;
        string message = "Condition should be false";
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => AssertionUtility.IsFalse(condition, message));
    }

    [Fact]
    public void IsNotNull_ShouldNotThrowWhenValueIsNotNull()
    {
        // Arrange
        object value = new object();
        string message = "Value should not be null";
        
        // Act & Assert - Should not throw
        AssertionUtility.IsNotNull(value, message);
    }

    [Fact]
    public void IsNotNull_ShouldThrowWhenValueIsNull()
    {
        // Arrange
        object value = null;
        string message = "Value should not be null";
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => AssertionUtility.IsNotNull(value, message));
    }

    [Fact]
    public void ResultNotNull_ShouldReturnValueWhenNotNull()
    {
        // Arrange
        string result = "test";
        string message = "Result should not be null";
        
        // Act
        var returnedResult = AssertionUtility.ResultNotNull(result, message);
        
        // Assert
        Assert.Equal(result, returnedResult);
    }

    [Fact]
    public void ResultNotNull_ShouldThrowWhenResultIsNull()
    {
        // Arrange
        string result = null;
        string message = "Result should not be null";
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => AssertionUtility.ResultNotNull(result, message));
    }
}
