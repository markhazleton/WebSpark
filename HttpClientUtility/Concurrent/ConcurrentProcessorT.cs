using System.Diagnostics;

namespace HttpClientUtility.Concurrent;

public abstract class ConcurrentProcessor<T> where T : ConcurrentProcessorModel
{
    private readonly Func<int, T> taskDataFactory;
    protected int MaxConcurrency;
    protected int MaxTaskCount;
    private readonly List<Task<T>> tasks;

    protected ConcurrentProcessor(Func<int, T> taskDataFactory)
    {
        MaxTaskCount = 1;
        MaxConcurrency = 1;
        tasks = [];
        this.taskDataFactory = taskDataFactory;
    }

    protected async Task<long> AwaitSemaphoreAsync(SemaphoreSlim semaphore, CancellationToken ct = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await semaphore.WaitAsync(ct);
        stopwatch.Stop();
        return stopwatch.ElapsedTicks;
    }

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

    protected abstract Task<T> ProcessAsync(T taskData, CancellationToken ct = default);

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

