using Microsoft.Extensions.Logging.Abstractions;
using PowerCSharp.Operational.Abstractions.Enums;

namespace PowerCSharp.Operational.Tests;

public class IssueManagerTests
{
    [Fact]
    public void CaptureException_ReturnsSameExceptionInstance_AndForwardsToDiagnostics()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders((DiagnosticHeaders.Debug, "true"));
        var diagnostics = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));
        var sut = new IssueManager(diagnostics, NullLogger<IssueManager>.Instance);

        Exception exception;
        try
        {
            throw new InvalidOperationException("boom");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var (returnedException, diagnosticEvent) = sut.CaptureException(exception);

        Assert.Same(exception, returnedException);
        Assert.NotNull(diagnosticEvent);
        Assert.Equal(DiagnosticEventType.Exception, diagnosticEvent!.Type);
    }

    [Fact]
    public void CaptureException_MergesDataDictionary_IntoExceptionData()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders((DiagnosticHeaders.Debug, "true"));
        var diagnostics = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));
        var sut = new IssueManager(diagnostics, NullLogger<IssueManager>.Instance);

        var exception = new InvalidOperationException("boom");
        var data = new Dictionary<string, object> { ["requestId"] = "abc-123" };

        var (returnedException, _) = sut.CaptureException(exception, data);

        Assert.Equal("abc-123", returnedException.Data["requestId"]);
    }

    [Fact]
    public void CaptureError_WhenDiagnosticsDisabled_ReturnsNull_AndDoesNotThrow()
    {
        var accessor = TestSupport.HttpContextAccessorWithNoContext();
        var diagnostics = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));
        var sut = new IssueManager(diagnostics, NullLogger<IssueManager>.Instance);

        var result = sut.CaptureError("something went wrong");

        Assert.Null(result);
    }

    [Fact]
    public void CaptureError_WhenDiagnosticsEnabled_ReturnsDiagnosticEvent()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders((DiagnosticHeaders.Debug, "true"));
        var diagnostics = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));
        var sut = new IssueManager(diagnostics, NullLogger<IssueManager>.Instance);

        var result = sut.CaptureError("something went wrong");

        Assert.NotNull(result);
        Assert.Equal(DiagnosticEventType.Error, result!.Type);
    }

    [Fact]
    public void AddBreadcrumb_ForwardsMessageAndCategory_ToDiagnostics()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders((DiagnosticHeaders.Debug, "true"));
        var diagnostics = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));
        var sut = new IssueManager(diagnostics, NullLogger<IssueManager>.Instance);

        var result = sut.AddBreadcrumb("user logged in", category: "auth");

        Assert.NotNull(result);
        Assert.Equal(DiagnosticEventType.Breadcrumb, result!.Type);
        Assert.Contains("auth", result.Message);
        Assert.Contains("user logged in", result.Message);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDiagnosticsServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new IssueManager(null!, NullLogger<IssueManager>.Instance));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var accessor = TestSupport.HttpContextAccessorWithNoContext();
        var diagnostics = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        Assert.Throws<ArgumentNullException>(() => new IssueManager(diagnostics, null!));
    }
}
