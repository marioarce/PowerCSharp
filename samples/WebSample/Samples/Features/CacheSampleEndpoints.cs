using PowerCSharp.Feature.Cache.Abstractions;

namespace WebSample.Samples.Features;

/// <summary>
/// Sample endpoint demonstrating the Cache feature, resolved via the Features Framework
/// (<c>PowerCSharp.Feature.Cache</c> + <c>PowerCSharp.Feature.Cache.BitFaster</c>).
/// </summary>
public static class CacheSampleEndpoints
{
    private const string DemoKey = "websample:cache:demo";

    /// <summary>
    /// Gets cache demo data: a cache miss, a write, and the resulting hit, using the
    /// DI-resolved <see cref="ICacheService"/> (the active BitFaster provider, or NoOp if the
    /// Cache feature is disabled via configuration).
    /// </summary>
    /// <param name="cache">The cache service, injected by the Features Framework.</param>
    /// <returns>Demo results showing a cache miss followed by a set/hit round-trip.</returns>
    public static async Task<object> GetDemoDataAsync(ICacheService cache)
    {
        // Start from a clean slate so this endpoint is idempotent across repeated calls.
        await cache.RemoveAsync(DemoKey);

        var missResult = await cache.GetWithResultAsync<string>(DemoKey);

        var value = $"cached at {DateTimeOffset.UtcNow:O}";
        await cache.SetAsync(DemoKey, value, TimeSpan.FromMinutes(1));

        var hitResult = await cache.GetWithResultAsync<string>(DemoKey);

        return new
        {
            providerNote = "Backed by PowerCSharp.Feature.Cache.BitFaster; falls back to a NoOp cache if the Cache feature is disabled.",
            beforeSet = new { hit = missResult.IsSuccess, value = missResult.Value },
            afterSet = new { hit = hitResult.IsSuccess, value = hitResult.Value, provider = hitResult.ProviderName }
        };
    }
}
