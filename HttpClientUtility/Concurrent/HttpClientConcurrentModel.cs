using HttpClientUtility.SendService;

namespace HttpClientUtility.Concurrent;

public class HttpClientConcurrentModel : ConcurrentProcessorModel
{
    public HttpClientConcurrentModel(int taskId, string requestUrl) : base(taskId)
    {
        statusCall = new HttpClientSendRequest<SiteStatus>(taskId, requestUrl);
        TaskId = taskId;
    }
    public HttpClientConcurrentModel(HttpClientConcurrentModel model, string endPoint) : base(model.TaskId)
    {
        statusCall = model.statusCall;
        TaskId = model.TaskId;
        TaskCount = model.TaskCount;
        DurationMS = model.DurationMS;
        SemaphoreCount = model.SemaphoreCount;
        SemaphoreWaitTicks = model.SemaphoreWaitTicks;
    }

    public HttpClientSendRequest<SiteStatus> statusCall { get; set; } = default!;
}
