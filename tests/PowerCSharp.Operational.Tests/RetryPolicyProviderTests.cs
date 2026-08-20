using Microsoft.Extensions.Logging.Abstractions;
using PowerCSharp.Operational.Policies.Retry;
using Polly;

namespace PowerCSharp.Operational.Tests;

/// <summary>
/// Tests exercise only the success path of each policy/pipeline — <see cref="RetryPolicyProvider"/>
/// detects the xunit test host and builds a no-op resilience pipeline (see
/// <c>RetryPolicyProvider.IsRunningInTestHost</c>), but the legacy <c>Polly.Policy</c>-based methods
/// (<see cref="RetryPolicyProvider.CreatePolicy"/>, <see cref="RetryPolicyProvider.GetAsyncPolicy"/>,
/// <see cref="RetryPolicyProvider.GetPolicy"/>) are not test-host-aware, so exercising their retry
/// path here would incur real exponential-backoff delays.
/// </summary>
public class RetryPolicyProviderTests
{
    [Fact]
    public void GetPipeline_ReturnsPipeline_ThatPassesSuccessfulResponseThrough()
    {
        var sut = new RetryPolicyProvider();
        var pipeline = sut.GetPipeline();

        Assert.NotNull(pipeline);

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        var result = pipeline.Execute(_ => response);

        Assert.Same(response, result);
    }

    [Fact]
    public void CreatePolicy_ReturnsNonNullAsyncPolicy()
    {
        var sut = new RetryPolicyProvider();

        var policy = sut.CreatePolicy("some-key");

        Assert.NotNull(policy);
    }

    [Fact]
    public async Task CreatePolicy_ExecutesSuccessfulOperation_WithoutRetrying()
    {
        var sut = new RetryPolicyProvider();
        var policy = sut.CreatePolicy("some-key");
        var callCount = 0;

        var result = await policy.ExecuteAsync(() =>
        {
            callCount++;
            return Task.FromResult("ok");
        });

        Assert.Equal("ok", result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetAsyncPolicy_ExecutesSuccessfulOperation_WithoutRetrying()
    {
        var sut = new RetryPolicyProvider();
        var policy = sut.GetAsyncPolicy(NullLogger.Instance, maxAttempts: 3);
        var callCount = 0;

        var result = await policy.ExecuteAsync(() =>
        {
            callCount++;
            return Task.FromResult(42);
        });

        Assert.Equal(42, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetPolicy_ExecutesSuccessfulOperation_WithoutRetrying()
    {
        var sut = new RetryPolicyProvider();
        var policy = sut.GetPolicy(NullLogger.Instance, maxAttempts: 3);
        var callCount = 0;

        var result = policy.Execute(() =>
        {
            callCount++;
            return "ok";
        });

        Assert.Equal("ok", result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetAsyncPolicy_AndGetPolicy_AcceptNullLogger_WithoutThrowing()
    {
        var sut = new RetryPolicyProvider();

        Assert.NotNull(sut.GetAsyncPolicy(null, maxAttempts: 1));
        Assert.NotNull(sut.GetPolicy(null, maxAttempts: 1));
    }
}
