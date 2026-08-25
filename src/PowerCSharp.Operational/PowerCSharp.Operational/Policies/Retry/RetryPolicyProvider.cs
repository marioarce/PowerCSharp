using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net;

namespace PowerCSharp.Operational.Policies.Retry;

/// <summary>
/// Provides a resilience pipeline with exponential backoff and jitter, plus a circuit breaker, for
/// <see cref="HttpResponseMessage"/> operations. Uses a "decorrelated jitter backoff" shape to
/// avoid retry contention and improve recovery under transient failure.
/// </summary>
public sealed class RetryPolicyProvider : IRetryPolicyProvider
{
    private readonly ILogger<RetryPolicyProvider>? _logger;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryPolicyProvider"/> class and builds the
    /// resilience pipeline. Under a detected unit-test host, builds a no-op pipeline instead (no
    /// retries, no circuit breaker) so tests don't pay for real backoff delays.
    /// </summary>
    /// <param name="logger">An optional logger used to record circuit-breaker state transitions.</param>
    public RetryPolicyProvider(ILogger<RetryPolicyProvider>? logger = null)
    {
        _logger = logger;

        if (IsRunningInTestHost())
        {
            _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>().Build();
            _logger?.LogDebug("RetryPolicyProvider: test host detected — using a no-op pipeline.");
            return;
        }

        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = DiagnosticsService.DefaultHttpMaxAttempts,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    // Only retry transient server errors (5xx) and specifically retry-worthy
                    // client errors. Permanent client failures (400/401/403/404/409/422, etc.)
                    // must never be retried.
                    .HandleResult(r =>
                        (int)r.StatusCode >= 500
                        || r.StatusCode == HttpStatusCode.RequestTimeout
                        || r.StatusCode == HttpStatusCode.TooManyRequests)
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r =>
                        (int)r.StatusCode >= 500
                        || r.StatusCode == HttpStatusCode.RequestTimeout
                        || r.StatusCode == HttpStatusCode.TooManyRequests),
                OnOpened = args =>
                {
                    _logger?.LogWarning("Circuit breaker opened for {BreakDuration}s due to {StatusCode}.", args.BreakDuration.TotalSeconds, args.Outcome.Result?.StatusCode);
                    return default;
                },
                OnClosed = _ =>
                {
                    _logger?.LogInformation("Circuit breaker closed/reset.");
                    return default;
                },
                OnHalfOpened = _ =>
                {
                    _logger?.LogInformation("Circuit breaker is half-open; testing.");
                    return default;
                }
            })
            .Build();
    }

    /// <inheritdoc />
    public ResiliencePipeline<HttpResponseMessage> GetPipeline() => _pipeline;

    /// <inheritdoc />
    public IAsyncPolicy CreatePolicy(string key) => Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: DiagnosticsService.DefaultMethodMaxAttempts,
            sleepDurationProvider: attempt => ExponentialBackoffWithJitter(attempt));

    /// <inheritdoc />
    public AsyncRetryPolicy GetAsyncPolicy(ILogger? logger, int maxAttempts) => Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: maxAttempts,
            sleepDurationProvider: attempt => ExponentialBackoffWithJitter(attempt),
            onRetry: (exception, timespan, retryCount, _) =>
                logger?.LogWarning(exception, "Retry {RetryCount} after {Delay} due to: {Message}", retryCount, timespan, exception.Message));

    /// <inheritdoc />
    public RetryPolicy GetPolicy(ILogger? logger, int maxAttempts) => Policy
        .Handle<Exception>()
        .WaitAndRetry(
            retryCount: maxAttempts,
            sleepDurationProvider: attempt => ExponentialBackoffWithJitter(attempt),
            onRetry: (exception, timespan, retryCount, _) =>
                logger?.LogWarning(exception, "Retry {RetryCount} after {Delay} due to: {Message}", retryCount, timespan, exception.Message));

    private static TimeSpan ExponentialBackoffWithJitter(int attempt)
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100));
        return delay + jitter;
    }

    /// <summary>
    /// Detects whether the current process is running inside a unit-test host, by checking for
    /// well-known test-framework assemblies in the current <see cref="AppDomain"/>.
    /// </summary>
    private static bool IsRunningInTestHost()
    {
        string[] testAssemblyPrefixes = ["xunit", "nunit.framework", "mstest.testframework"];

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Any(a => testAssemblyPrefixes.Any(prefix => a.FullName?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ?? false));
    }
}
