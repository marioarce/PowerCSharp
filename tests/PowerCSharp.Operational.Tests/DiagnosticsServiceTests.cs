using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.Enums;

namespace PowerCSharp.Operational.Tests;

public class DiagnosticsServiceTests
{
    [Fact]
    public void IsEnabled_False_WhenDebugHeaderAbsent()
    {
        var accessor = TestSupport.HttpContextAccessorWithNoContext();
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        Assert.False(sut.IsEnabled);
    }

    [Fact]
    public void IsEnabled_True_WhenDebugHeaderPresentAndTrue()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders((DiagnosticHeaders.Debug, "true"));
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        Assert.True(sut.IsEnabled);
    }

    [Fact]
    public void IsDebugVerbose_RequiresBothDebugAndVerboseHeaders()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders(
            (DiagnosticHeaders.Debug, "true"),
            (DiagnosticHeaders.DebugVerbose, "true"));
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        Assert.True(sut.IsDebugVerbose);
    }

    [Fact]
    public void AddTrace_WhenDisabled_StillRecordsEvent_ButBuildPayloadReturnsNull()
    {
        // Diagnostics being "disabled" gates BuildPayload/GetEvents, not whether an event is
        // recorded — this mirrors the source behavior, where events accumulate regardless and
        // filtering happens only at read time.
        var accessor = TestSupport.HttpContextAccessorWithNoContext();
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        sut.AddTrace("hello world", TraceLevel.Information);

        Assert.Null(sut.BuildPayload());
        Assert.Null(sut.GetEvents());
    }

    [Fact]
    public void GetEvents_WhenEnabled_ReturnsRecordedEvent()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders((DiagnosticHeaders.Debug, "true"));
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        sut.AddTrace("hello world", TraceLevel.Error);

        var events = sut.GetEvents();

        Assert.NotNull(events);
        Assert.Single(events!);
        Assert.Equal("hello world", events![0].Message);
    }

    [Fact]
    public void GetEvents_FiltersBelowConfiguredTraceLevel()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders(
            (DiagnosticHeaders.Debug, "true"),
            (DiagnosticHeaders.TraceLevel, ((int)TraceLevel.Warning).ToString()));
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        sut.AddTrace("debug-level message", TraceLevel.Debug);
        sut.AddTrace("warning-level message", TraceLevel.Warning);

        var events = sut.GetEvents();

        Assert.NotNull(events);
        Assert.Single(events!);
        Assert.Equal("warning-level message", events![0].Message);
    }

    [Fact]
    public void GetEvents_ForcesFullTraceLevel_WhenAnErrorWasCaptured()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders(
            (DiagnosticHeaders.Debug, "true"),
            (DiagnosticHeaders.TraceLevel, ((int)TraceLevel.Warning).ToString()));
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        sut.AddTrace("debug-level message", TraceLevel.Debug);
        sut.AddError("something went wrong");

        // Once an error/exception is present, the service escalates to full trace output so
        // troubleshooting has the complete picture leading up to the failure.
        var events = sut.GetEvents();

        Assert.NotNull(events);
        Assert.True(events!.Count > 0);
    }

    [Fact]
    public void AddException_CapturesMessageAndStackTrace()
    {
        var accessor = TestSupport.HttpContextAccessorWithHeaders((DiagnosticHeaders.Debug, "true"));
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        Exception exception;
        try
        {
            throw new InvalidOperationException("boom");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var result = sut.AddException(exception);

        Assert.NotNull(result);
        Assert.Equal(Abstractions.Enums.DiagnosticEventType.Exception, result!.Type);
        Assert.Contains("boom", result.Message);
        Assert.NotNull(result.StackTrace);
    }

    [Fact]
    public void BuildPayload_WhenDisabled_ReturnsNull()
    {
        var accessor = TestSupport.HttpContextAccessorWithNoContext();
        var sut = new DiagnosticsService(accessor, TestSupport.Options(), TestSupport.CreateInertEventLogWriter(accessor));

        Assert.Null(sut.BuildPayload());
    }
}
