using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace PowerCSharp.Operational.Policies.Retry;

/// <summary>
/// Supplies resilience pipelines/policies for HTTP and general method retries. Lives in the core
/// <c>PowerCSharp.Operational</c> package (not <c>PowerCSharp.Operational.Abstractions</c>) because
/// its shape is defined in terms of Polly types — putting it in Abstractions would leak a
/// third-party dependency into consumers who only want the zero-dependency contracts.
/// </summary>
public interface IRetryPolicyProvider
{
    /// <summary>Gets the shared HTTP resilience pipeline (retry + circuit breaker).</summary>
    ResiliencePipeline<HttpResponseMessage> GetPipeline();

    /// <summary>Creates a keyed async retry policy for general (non-HTTP) operations.</summary>
    IAsyncPolicy CreatePolicy(string key);

    /// <summary>Gets an async retry policy with the given max attempts, logging retries via <paramref name="logger"/>.</summary>
    AsyncRetryPolicy GetAsyncPolicy(ILogger? logger, int maxAttempts);

    /// <summary>Gets a synchronous retry policy with the given max attempts, logging retries via <paramref name="logger"/>.</summary>
    RetryPolicy GetPolicy(ILogger? logger, int maxAttempts);
}
