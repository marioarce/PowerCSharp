using Microsoft.Extensions.Logging.Abstractions;
using PowerCSharp.Operational.Abstractions.Models;
using PowerCSharp.Operational.Abstractions.NoOp;

namespace PowerCSharp.Operational.Tests;

/// <summary>
/// Verifies the NoOp floors are genuinely inert — the core "connect/disconnect without crashing"
/// guarantee that Operational.Abstractions exists to provide.
/// </summary>
public class NoOpTests
{
    [Fact]
    public void NoOpDiagnosticsService_ReportsDisabled()
    {
        var sut = new NoOpDiagnosticsService(NullLogger<NoOpDiagnosticsService>.Instance);

        Assert.False(sut.IsEnabled);
        Assert.False(sut.IsVerbose);
        Assert.False(sut.IsEventLogEnabled);
        Assert.False(sut.IsCacheDisabled);
        Assert.False(sut.IsPerformanceEnabled);
    }

    [Fact]
    public void NoOpDiagnosticsService_AddMethods_ReturnNull()
    {
        var sut = new NoOpDiagnosticsService(NullLogger<NoOpDiagnosticsService>.Instance);

        Assert.Null(sut.AddTrace("message"));
        Assert.Null(sut.AddBreadcrumb("message", "category"));
        Assert.Null(sut.AddException(new InvalidOperationException("boom")));
        Assert.Null(sut.AddError("error"));
        Assert.Null(sut.GetEvents());
        Assert.Null(sut.BuildPayload());
    }

    [Fact]
    public void NoOpDiagnosticsService_Obfuscate_ReturnsInputUnchanged()
    {
        var sut = new NoOpDiagnosticsService(NullLogger<NoOpDiagnosticsService>.Instance);
        object input = "sensitive-value";

        Assert.Same(input, sut.Obfuscate(input));
    }

    [Fact]
    public void NoOpIssueManager_CaptureException_ReturnsExceptionUnmodifiedAndNullEvent()
    {
        var sut = new NoOpIssueManager(NullLogger<NoOpIssueManager>.Instance);
        var exception = new InvalidOperationException("boom");

        var (returnedException, diagnosticEvent) = sut.CaptureException(exception);

        Assert.Same(exception, returnedException);
        Assert.Null(diagnosticEvent);
    }

    [Fact]
    public void NoOpIssueManager_CaptureErrorAndAddBreadcrumb_ReturnNull()
    {
        var sut = new NoOpIssueManager(NullLogger<NoOpIssueManager>.Instance);

        Assert.Null(sut.CaptureError("error"));
        Assert.Null(sut.AddBreadcrumb("message"));
    }

    [Fact]
    public void NoOpEventViewerService_TryEnqueue_AlwaysSucceeds()
    {
        var sut = new NoOpEventViewerService();
        var entry = new EventViewerLogEntry(DateTime.UtcNow, "Category", Microsoft.Extensions.Logging.LogLevel.Information, "message", null, null);

        Assert.True(sut.TryEnqueue(entry));
    }
}
