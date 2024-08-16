
using System.Diagnostics;

namespace AsyncSpark.Services;

/// <summary>
/// Samples for Async Methods
/// Supporting Video: https://channel9.msdn.com/Series/Three-Essential-Tips-for-Async/Three-Essential-Tips-For-Async-Introduction
/// Supporting Blog: https://johnthiriet.com/configure-await/
/// https://docs.microsoft.com/en-us/shows/three-essential-tips-for-async/
/// </summary>
public class AsyncMockService
{

    public static async Task ExampleMethodAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            // Simulate work
            await Task.Delay(1000, ct);
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
            var result = await task;

            // Set the taskCompletionSource result
            taskCompletionSource.TrySetResult(result);
        }
        // Return the result of the TaskCompletionSource.Task
        return await taskCompletionSource.Task;
    }
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
                    await PerformTaskAsync(name, delay, throwEx, ct);
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
    public static async Task PerformTaskAsync(string name, int delay, bool throwEx, CancellationToken ct = default)
    {
        await Task.Delay(delay, ct);
        ct.ThrowIfCancellationRequested();

        if (throwEx)
        {
            throw new Exception($"{name} PerformTaskAsync Exception.");
        }
    }
}
