using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.NoOp;
using PowerCSharp.Operational.EventLog;

namespace PowerCSharp.Operational.Tests;

/// <summary>Shared test helpers for building minimal HTTP context, options, and writer fakes.</summary>
internal static class TestSupport
{
    /// <summary>Builds an <see cref="IHttpContextAccessor"/> whose request carries the given headers.</summary>
    public static IHttpContextAccessor HttpContextAccessorWithHeaders(params (string Key, string Value)[] headers)
    {
        var context = new DefaultHttpContext();

        foreach (var (key, value) in headers)
        {
            context.Request.Headers[key] = value;
        }

        return new HttpContextAccessor { HttpContext = context };
    }

    /// <summary>Builds an <see cref="IHttpContextAccessor"/> with no active HTTP context.</summary>
    public static IHttpContextAccessor HttpContextAccessorWithNoContext() => new HttpContextAccessor { HttpContext = null };

    /// <summary>Wraps an <see cref="OperationalOptions"/> instance as <see cref="IOptions{TOptions}"/>.</summary>
    public static IOptions<OperationalOptions> Options(OperationalOptions? options = null) =>
        Microsoft.Extensions.Options.Options.Create(options ?? new OperationalOptions());

    /// <summary>
    /// Builds an <see cref="EventLogWriter"/> with no configured <c>LogsBasePath</c>, so
    /// <c>Enqueue</c> is a safe no-op — suitable for tests that don't exercise disk writing itself.
    /// Callers should dispose the result to stop its background task promptly.
    /// </summary>
    public static EventLogWriter CreateInertEventLogWriter(IHttpContextAccessor? accessor = null) =>
        new(accessor ?? HttpContextAccessorWithNoContext(), new NoOpEventViewerService(), Options(), NullLogger<EventLogWriter>.Instance);
}
