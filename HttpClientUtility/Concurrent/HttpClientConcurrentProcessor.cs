using System.Diagnostics;

namespace HttpClientUtility.Concurrent;

/// <summary>
/// Represents a concurrent processor that processes tasks in parallel.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConcurrentProcessor{T}"/> class.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpClientConcurrentProcessor"/> class.
/// </remarks>
/// <param name="taskDataFactory">The factory function to create task data.</param>
/// <param name="service">The HttpClientService instance.</param>
public class HttpClientConcurrentProcessor(Func<int, HttpClientConcurrentModel> taskDataFactory, SendService.IHttpClientSendService service) : ConcurrentProcessor<HttpClientConcurrentModel>(taskDataFactory)
{

    /// <summary>
    /// Gets the next task data based on the current task data.
    /// </summary>
    /// <param name="taskData">The current task data.</param>
    /// <returns>The next task data or null if there are no more tasks.</returns>
    protected override HttpClientConcurrentModel? GetNextTaskData(HttpClientConcurrentModel taskData)
    {
        if (taskData.TaskId < MaxTaskCount)
        {
            return new HttpClientConcurrentModel(taskData.TaskId + 1, taskData.StatusCall.RequestPath);
        }
        else return null;
    }

    /// <summary>
    /// Processes the task asynchronously.
    /// </summary>
    /// <param name="taskData">The task data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the task.</returns>
    protected override async Task<HttpClientConcurrentModel> ProcessAsync(HttpClientConcurrentModel taskData, CancellationToken ct = default)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var result = await service.HttpClientSendAsync(taskData.StatusCall, ct).ConfigureAwait(false);
        taskData.StatusCall = result;
        sw.Stop();
        taskData.DurationMS = sw.ElapsedMilliseconds;
        return new HttpClientConcurrentModel(taskData, taskData.StatusCall.RequestPath);
    }
}
