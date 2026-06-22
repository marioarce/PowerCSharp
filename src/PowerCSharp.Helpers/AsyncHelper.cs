using System;
using System.Threading;
using System.Threading.Tasks;

namespace PowerCSharp.Helpers;

/// <summary>
/// Provides utilities for safely bridging asynchronous operations to synchronous contexts.
/// </summary>
/// <remarks>
/// This helper is designed for scenarios where async operations must be called from sync code,
/// such as in MVC filters or other synchronous frameworks. Uses a custom TaskFactory
/// to avoid deadlocks and ensure proper thread scheduling.
/// </remarks>
public static class AsyncHelper
{
    /// <summary>
    /// Task factory configured to prevent deadlocks in sync-over-async scenarios.
    /// </summary>
    /// <remarks>
    /// Uses default task scheduler and no cancellation to ensure predictable behavior.
    /// </remarks>
    private static readonly TaskFactory _taskFactory =
        new(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

    /// <summary>
    /// Runs an asynchronous function synchronously and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type of the asynchronous operation.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>The result of the asynchronous operation.</returns>
    /// <remarks>
    /// This method safely bridges async to sync without causing deadlocks.
    /// Use only when absolutely necessary, such as in synchronous frameworks like MVC filters.
    /// </remarks>
    public static T RunSync<T>(Func<Task<T>> func)
        => _taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();

    /// <summary>
    /// Runs an asynchronous function that returns a ValueTask synchronously and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type of the asynchronous operation.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>The result of the asynchronous operation.</returns>
    /// <remarks>
    /// This method safely bridges async to sync without causing deadlocks.
    /// Use only when absolutely necessary, such as in synchronous frameworks like MVC filters.
    /// </remarks>
    public static T RunSync<T>(Func<ValueTask<T>> func)
        => _taskFactory.StartNew(async () => await func()).Unwrap().GetAwaiter().GetResult();

    /// <summary>
    /// Runs an asynchronous function that returns a ValueTask synchronously.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <remarks>
    /// This method safely bridges async to sync without causing deadlocks.
    /// Use only when absolutely necessary, such as in synchronous frameworks like MVC filters.
    /// </remarks>
    public static void RunSync(Func<ValueTask> func)
        => _taskFactory.StartNew(async () => await func()).Unwrap().GetAwaiter().GetResult();
}
