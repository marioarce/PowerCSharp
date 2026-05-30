using PowerCSharp.Extensions;
using Xunit;

namespace PowerCSharp.Extensions.Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void GetAge_ShouldCalculateCorrectAge()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-25);
        
        // Act
        int age = birthDate.GetAge();
        
        // Assert
        Assert.Equal(25, age);
    }
    
    [Fact]
    public void GetAge_ShouldCalculateAgeForBirthdayNotYetThisYear()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-25).AddDays(1); // Birthday is tomorrow
        
        // Act
        int age = birthDate.GetAge();
        
        // Assert
        Assert.Equal(24, age);
    }
    
    [Fact]
    public void GetAge_ShouldCalculateAgeForBirthdayToday()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-30);
        
        // Act
        int age = birthDate.GetAge();
        
        // Assert
        Assert.Equal(30, age);
    }
    
    [Fact]
    public void IsWeekend_ShouldReturnTrueForSaturday()
    {
        // Arrange
        var saturday = new DateTime(2026, 5, 30); // This is a Saturday
        
        // Act
        bool result = saturday.IsWeekend();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsWeekend_ShouldReturnTrueForSunday()
    {
        // Arrange
        var sunday = new DateTime(2026, 5, 31); // This is a Sunday
        
        // Act
        bool result = sunday.IsWeekend();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsWeekend_ShouldReturnFalseForMonday()
    {
        // Arrange
        var monday = new DateTime(2026, 6, 1); // This is a Monday
        
        // Act
        bool result = monday.IsWeekend();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsWeekend_ShouldReturnFalseForFriday()
    {
        // Arrange
        var friday = new DateTime(2026, 5, 29); // This is a Friday
        
        // Act
        bool result = friday.IsWeekend();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void FirstDayOfMonth_ShouldReturnFirstDay()
    {
        // Arrange
        var date = new DateTime(2026, 5, 15);
        
        // Act
        var result = date.FirstDayOfMonth();
        
        // Assert
        Assert.Equal(new DateTime(2026, 5, 1), result);
    }
    
    [Fact]
    public void FirstDayOfMonth_ShouldWorkForJanuary()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        
        // Act
        var result = date.FirstDayOfMonth();
        
        // Assert
        Assert.Equal(new DateTime(2026, 1, 1), result);
    }
    
    [Fact]
    public void FirstDayOfMonth_ShouldWorkForDecember()
    {
        // Arrange
        var date = new DateTime(2026, 12, 15);
        
        // Act
        var result = date.FirstDayOfMonth();
        
        // Assert
        Assert.Equal(new DateTime(2026, 12, 1), result);
    }
    
    [Fact]
    public void LastDayOfMonth_ShouldReturnLastDayForMonthWith31Days()
    {
        // Arrange
        var date = new DateTime(2026, 5, 15); // May has 31 days
        
        // Act
        var result = date.LastDayOfMonth();
        
        // Assert
        Assert.Equal(new DateTime(2026, 5, 31), result);
    }
    
    [Fact]
    public void LastDayOfMonth_ShouldReturnLastDayForMonthWith30Days()
    {
        // Arrange
        var date = new DateTime(2026, 4, 15); // April has 30 days
        
        // Act
        var result = date.LastDayOfMonth();
        
        // Assert
        Assert.Equal(new DateTime(2026, 4, 30), result);
    }
    
    [Fact]
    public void LastDayOfMonth_ShouldReturnLastDayForFebruaryInNonLeapYear()
    {
        // Arrange
        var date = new DateTime(2026, 2, 15); // 2026 is not a leap year
        
        // Act
        var result = date.LastDayOfMonth();
        
        // Assert
        Assert.Equal(new DateTime(2026, 2, 28), result);
    }
    
    [Fact]
    public void LastDayOfMonth_ShouldReturnLastDayForFebruaryInLeapYear()
    {
        // Arrange
        var date = new DateTime(2024, 2, 15); // 2024 is a leap year
        
        // Act
        var result = date.LastDayOfMonth();
        
        // Assert
        Assert.Equal(new DateTime(2024, 2, 29), result);
    }
}
