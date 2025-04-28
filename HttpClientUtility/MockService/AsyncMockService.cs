using System.Diagnostics;

namespace HttpClientUtility.MockService;

/// <summary>
/// Samples for Async Methods
/// Supporting Video: https://channel9.msdn.com/Series/Three-Essential-Tips-for-Async/Three-Essential-Tips-For-Async-Introduction
/// Supporting Blog: https://johnthiriet.com/configure-await/
/// https://docs.microsoft.com/en-us/shows/three-essential-tips-for-async/
/// </summary>
public class AsyncMockService : IAsyncMockService
{
    /// <summary>
    /// Example method demonstrating a simple asynchronous operation with cancellation support.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task ExampleMethodAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            // Simulate work
            await Task.Delay(1000, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Compute a value for a long time.
    /// </summary>
    /// <returns>The value computed.</returns>
    /// <param name="loop">Number of iterations to do.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static Task<decimal> LongRunningCancellableOperation(int loop, CancellationToken cancellationToken)
    {
        Task<decimal>? task = null;
        // Start a task a return it
        task = Task.Run(() =>
        {
            decimal result = 0;
            // Loop for a defined number of iterations
            for (int i = 0; i < loop; i++)
            {
                // Check if a cancellation is requested, if yes,
                // throw a TaskCanceledException.
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException(task);


                cancellationToken.ThrowIfCancellationRequested();


                // Do something that takes times.
                Thread.Sleep(i);
                result += i;
            }
            return result;
        });
        return task;
    }

    /// <summary>
    /// Compute a value for a long time.
    /// </summary>
    /// <returns>The value computed.</returns>
    /// <param name="loop">Number of iterations to do.</param>
    public Task<decimal> LongRunningOperation(int loop)
    {
        // Start a task a return it
        return Task.Run(() =>
        {
            decimal result = 0;

            // Loop for a defined number of iterations
            for (int i = 0; i < loop; i++)
            {
                // Do something that takes a long time (i.e. sleep) 
                Thread.Sleep(10);
                result += i;
            }
            return result;
        });
    }

    /// <summary>
    /// Performs a long-running operation with cancellation support using a TaskCompletionSource.
    /// </summary>
    /// <param name="loop">Number of iterations to perform</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>A task representing the asynchronous operation with a decimal result</returns>
    public async Task<decimal> LongRunningOperationWithCancellationTokenAsync(int loop,
                                                                              CancellationToken cancellationToken)
    {
        // We create a TaskCompletionSource of decimal
        var taskCompletionSource = new TaskCompletionSource<decimal>();

        // Registering a lambda into the cancellationToken
        cancellationToken.Register(() =>
        {
            // We received a cancellation message, cancel the TaskCompletionSource.Task
            taskCompletionSource.TrySetCanceled();
        });

        var task = LongRunningOperation(loop);

        // Wait for the first task to finish among the two
        var completedTask = await Task.WhenAny(task, taskCompletionSource.Task).ConfigureAwait(false);

        // If the completed task is our long running operation we set its result.
        if (completedTask == task)
        {
            // Extract the result, the task is finished and the await will return immediately
            var result = await task.ConfigureAwait(false);

            // Set the taskCompletionSource result
            taskCompletionSource.TrySetResult(result);
        }
        // Return the result of the TaskCompletionSource.Task
        return await taskCompletionSource.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a long-running task with support for cancellation, logging, and error handling.
    /// </summary>
    /// <param name="name">Name of the task for logging purposes</param>
    /// <param name="delay">Delay in milliseconds between iterations</param>
    /// <param name="iterations">Number of iterations to perform</param>
    /// <param name="throwEx">Whether to throw an exception during execution</param>
    /// <param name="logger">Logger to record events and exceptions</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="TimeoutException">Thrown when the task is cancelled</exception>
    public static async Task LongRunningTask(
        string name,
        int delay,
        int iterations,
        bool throwEx,
        ICommonLogger logger,
        CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();

        while (true)
        {
            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    await PerformTaskAsync(name, delay, throwEx, ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException ex)
                {
                    sw.Stop();
                    logger.TrackEvent($"{name} TaskCanceledException. {i} of {iterations} completed. Time:{sw.ElapsedMilliseconds}  Exception:{ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    logger.TrackException(ex, $"{name} Exception. {i} of {iterations} completed. Time:{sw.ElapsedMilliseconds} Exception:{ex.Message}");
                    throw;
                }
                finally
                {

                }
                // Check for cancellation after the delay
                if (ct.IsCancellationRequested)
                {
                    sw.Stop();
                    logger.TrackEvent($"{name} ct.IsCancellationRequested. {i} of {iterations} completed. Time:{sw.ElapsedMilliseconds}");
                    throw new TimeoutException($"{name} Long running task was cancelled.");
                }
            }
            sw.Stop();
            logger.TrackEvent($"{name} completed. Time:{sw.ElapsedMilliseconds}");
            return;
        }
    }

    /// <summary>
    /// Performs a simple task with configurable delay and exception throwing.
    /// </summary>
    /// <param name="name">Name of the task for error reporting</param>
    /// <param name="delay">Delay in milliseconds</param>
    /// <param name="throwEx">Whether to throw an exception after the delay</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Thrown when throwEx is true</exception>
    public static async Task PerformTaskAsync(string name, int delay, bool throwEx, CancellationToken ct = default)
    {
        await Task.Delay(delay, ct).ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();

        if (throwEx)
        {
            throw new Exception($"{name} PerformTaskAsync Exception.");
        }
    }
}
