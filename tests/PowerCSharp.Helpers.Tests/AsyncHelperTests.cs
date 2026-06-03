using System.Threading.Tasks;
using PowerCSharp.Helpers;
using Xunit;

namespace PowerCSharp.Helpers.Tests;

public class AsyncHelperTests
{
    [Fact]
    public void RunSync_WithSuccessfulTask_ShouldReturnResult()
    {
        // Arrange
        var expected = "Test result";
        Func<Task<string>> asyncFunc = async () =>
        {
            await Task.Delay(50);
            return expected;
        };

        // Act
        var result = AsyncHelper.RunSync(asyncFunc);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RunSync_WithIntTask_ShouldReturnIntResult()
    {
        // Arrange
        var expected = 42;
        Func<Task<int>> asyncFunc = async () =>
        {
            await Task.Delay(10);
            return expected;
        };

        // Act
        var result = AsyncHelper.RunSync(asyncFunc);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RunSync_WithBoolTask_ShouldReturnBoolResult()
    {
        // Arrange
        var expected = true;
        Func<Task<bool>> asyncFunc = async () =>
        {
            await Task.Delay(10);
            return expected;
        };

        // Act
        var result = AsyncHelper.RunSync(asyncFunc);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RunSync_WithVoidTask_ShouldCompleteSuccessfully()
    {
        // Arrange
        var isCompleted = false;
        Func<Task<object>> asyncFunc = async () =>
        {
            await Task.Delay(50);
            isCompleted = true;
            return new object();
        };

        // Act
        AsyncHelper.RunSync(asyncFunc);

        // Assert
        Assert.True(isCompleted);
    }

    [Fact]
    public void RunSync_WithException_ShouldThrowException()
    {
        // Arrange
        var expectedMessage = "Test exception";
        Func<Task<string>> asyncFunc = async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException(expectedMessage);
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => AsyncHelper.RunSync(asyncFunc));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void RunSync_WithNullFunction_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<Task<string>>? nullFunc = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AsyncHelper.RunSync<string>(nullFunc!));
    }

    [Fact]
    public void RunSync_WithNestedAsyncOperations_ShouldWorkCorrectly()
    {
        // Arrange
        Func<Task<string>> nestedAsyncFunc = async () =>
        {
            await Task.Delay(10);
            var innerResult = await GetInnerResultAsync();
            return $"Outer: {innerResult}";
        };

        // Act
        var result = AsyncHelper.RunSync(nestedAsyncFunc);

        // Assert
        Assert.Equal("Outer: Inner Result", result);
    }

    [Fact]
    public void RunSync_WithMultipleCalls_ShouldWorkConsistently()
    {
        // Arrange
        Func<Task<int>> asyncFunc = async () =>
        {
            await Task.Delay(10);
            return DateTime.Now.Millisecond;
        };

        // Act
        var result1 = AsyncHelper.RunSync(asyncFunc);
        var result2 = AsyncHelper.RunSync(asyncFunc);
        var result3 = AsyncHelper.RunSync(asyncFunc);

        // Assert
        Assert.True(result1 >= 0);
        Assert.True(result2 >= 0);
        Assert.True(result3 >= 0);
    }

    private static async Task<string> GetInnerResultAsync()
    {
        await Task.Delay(10);
        return "Inner Result";
    }
}
