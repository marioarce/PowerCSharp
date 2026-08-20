using Microsoft.Extensions.Logging.Abstractions;
using PowerCSharp.Operational.Abstractions;
using PowerCSharp.Operational.Abstractions.NoOp;
using PowerCSharp.Operational.EventLog;

namespace PowerCSharp.Operational.Tests;

public class EventLogWriterTests
{
    [Fact]
    public void Enqueue_WithNoBasePathConfigured_IsInert_AndDoesNotThrow()
    {
        using var sut = new EventLogWriter(
            TestSupport.HttpContextAccessorWithNoContext(),
            new NoOpEventViewerService(),
            TestSupport.Options(),
            NullLogger<EventLogWriter>.Instance);

        var exception = Record.Exception(() => sut.Enqueue(new { message = "hello" }));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Enqueue_WithBasePathConfigured_WritesNdjsonFileUnderAppNameDateFolder()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "pwcs-eventlog-tests-" + Guid.NewGuid().ToString("N"));

        try
        {
            var options = TestSupport.Options(new OperationalOptions
            {
                LogsBasePath = tempRoot,
                AppName = "TestApp"
            });

            using var sut = new EventLogWriter(
                TestSupport.HttpContextAccessorWithNoContext(),
                new NoOpEventViewerService(),
                options,
                NullLogger<EventLogWriter>.Instance);

            sut.Enqueue(new { message = "hello world" });

            var appFolder = Path.Combine(tempRoot, "TestApp");
            var writtenFile = await WaitForFileAsync(appFolder, TimeSpan.FromSeconds(5));

            Assert.NotNull(writtenFile);

            var content = await File.ReadAllTextAsync(writtenFile!);
            Assert.Contains("hello world", content);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void Dispose_CompletesQueue_WithoutThrowing()
    {
        var sut = new EventLogWriter(
            TestSupport.HttpContextAccessorWithNoContext(),
            new NoOpEventViewerService(),
            TestSupport.Options(),
            NullLogger<EventLogWriter>.Instance);

        var exception = Record.Exception(sut.Dispose);

        Assert.Null(exception);
    }

    /// <summary>Polls a directory tree for the first file to appear, up to a timeout, since writing happens on a background task.</summary>
    private static async Task<string?> WaitForFileAsync(string rootFolder, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (Directory.Exists(rootFolder))
            {
                var files = Directory.EnumerateFiles(rootFolder, "*.ndjson", SearchOption.AllDirectories).ToList();
                if (files.Count > 0)
                {
                    return files[0];
                }
            }

            await Task.Delay(50);
        }

        return null;
    }
}
