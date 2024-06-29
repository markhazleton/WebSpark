using System.Diagnostics;

namespace HttpClientUtility.Concurrent;

public class HttpClientConcurrentProcessor : ConcurrentProcessor<HttpClientConcurrentModel>
{
    private readonly Interfaces.IHttpClientService _service;

    public HttpClientConcurrentProcessor(
        Func<int, HttpClientConcurrentModel> taskDataFactory, Interfaces.IHttpClientService service) :
        base(
            taskDataFactory)
    {
        _service = service;
    }

    protected override HttpClientConcurrentModel? GetNextTaskData(HttpClientConcurrentModel taskData)
    {
        if (taskData.TaskId < MaxTaskCount)
        {
            return new HttpClientConcurrentModel(taskData.TaskId + 1, taskData.statusCall.RequestPath);
        }
        else return null;
    }

    protected override async Task<HttpClientConcurrentModel> ProcessAsync(HttpClientConcurrentModel taskData, CancellationToken ct = default)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var result = await _service.HttpClientSendAsync(taskData.statusCall, ct).ConfigureAwait(false);
        taskData.statusCall = result;
        sw.Stop();
        taskData.DurationMS = sw.ElapsedMilliseconds;
        return new HttpClientConcurrentModel(taskData, taskData.statusCall.RequestPath);
    }
}
