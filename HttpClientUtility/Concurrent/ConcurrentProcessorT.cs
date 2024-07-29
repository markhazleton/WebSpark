using System.Diagnostics;

namespace HttpClientUtility.Concurrent;

/// <summary>
/// Represents a concurrent processor that processes tasks in parallel.
/// </summary>
/// <typeparam name="T">The type of the task data.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ConcurrentProcessor{T}"/> class.
/// </remarks>
/// <param name="taskDataFactory">The factory function to create task data.</param>
public abstract class ConcurrentProcessor<T>(Func<int, T> taskDataFactory) where T : ConcurrentProcessorModel
{
    /// <summary>
    /// Max Concurrency
    /// </summary>
    protected int MaxConcurrency = 1;
    /// <summary>
    /// Max Task Count
    /// </summary>
    protected int MaxTaskCount = 1;
    private readonly List<Task<T>> tasks = [];

    /// <summary>
    /// Asynchronously waits for a semaphore and returns the elapsed ticks.
    /// </summary>
    /// <param name="semaphore">The semaphore to wait on.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The elapsed ticks.</returns>
    protected async Task<long> AwaitSemaphoreAsync(SemaphoreSlim semaphore, CancellationToken ct = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await semaphore.WaitAsync(ct);
        stopwatch.Stop();
        return stopwatch.ElapsedTicks;
    }

    /// <summary>
    /// Gets the next task data based on the current task data.
    /// </summary>
    /// <param name="taskData">The current task data.</param>
    /// <returns>The next task data or null if there are no more tasks.</returns>
    protected virtual T? GetNextTaskData(T taskData)
    {
        if (taskData.TaskId < MaxTaskCount)
        {
            int nextTaskId = taskData.TaskId + 1;
            T nextTaskData = taskDataFactory(nextTaskId);
            return nextTaskData;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Manages the process for a single task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="taskCount">The total number of tasks.</param>
    /// <param name="semaphoreWait">The elapsed ticks while waiting for the semaphore.</param>
    /// <param name="semaphore">The semaphore.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the task.</returns>
    protected async Task<T> ManageProcessAsync(int taskId, int taskCount, long semaphoreWait, SemaphoreSlim semaphore, CancellationToken ct = default)
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        try
        {
            T taskData = taskDataFactory(taskId);
            taskData.TaskCount = taskCount;
            taskData.SemaphoreCount = semaphore.CurrentCount;
            taskData.SemaphoreWaitTicks = semaphoreWait;

            var result = await ProcessAsync(taskData, ct);
            return result;
        }
        finally
        {
            semaphore.Release();
            sw.Stop();
        }
    }

    /// <summary>
    /// Processes the task asynchronously.
    /// </summary>
    /// <param name="taskData">The task data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the task.</returns>
    protected abstract Task<T> ProcessAsync(T taskData, CancellationToken ct = default);

    /// <summary>
    /// Runs the concurrent processor asynchronously.
    /// </summary>
    /// <param name="maxTaskCount">The maximum number of tasks to process.</param>
    /// <param name="maxConcurrency">The maximum concurrency level.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of results from the processed tasks.</returns>
    public async Task<List<T>> RunAsync(int maxTaskCount, int maxConcurrency, CancellationToken ct = default)
    {
        MaxTaskCount = maxTaskCount;
        MaxConcurrency = maxConcurrency;
        SemaphoreSlim semaphore = new(MaxConcurrency, MaxConcurrency);
        var taskData = taskDataFactory(1);
        List<T> results = [];
        while (taskData is not null)
        {
            long semaphoreWait = await AwaitSemaphoreAsync(semaphore, ct);
            Task<T> task = ManageProcessAsync(taskData.TaskId, tasks.Count, semaphoreWait, semaphore);
            tasks.Add(task);

            taskData = GetNextTaskData(taskDataFactory(taskData.TaskId));

            if (tasks.Count >= MaxConcurrency)
            {
                Task<T> finishedTask = await Task.WhenAny(tasks);
                results.Add(await finishedTask);
                tasks.Remove(finishedTask);
            }
        }
        await Task.WhenAll(tasks);
        foreach (var task in tasks)
        {
            results.Add(await task);
        }
        return results;
    }
}

