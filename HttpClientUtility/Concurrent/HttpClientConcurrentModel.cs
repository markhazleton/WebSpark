using HttpClientUtility.SendService;

namespace HttpClientUtility.Concurrent;

/// <summary>
/// Represents a model for concurrent processing tasks.
/// </summary>
public class HttpClientConcurrentModel : ConcurrentProcessorModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientConcurrentModel"/> class with the specified task ID and request URL.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="requestUrl">The request URL.</param>
    public HttpClientConcurrentModel(int taskId, string requestUrl) : base(taskId)
    {
        StatusCall = new HttpClientSendRequest<SiteStatus>(taskId, requestUrl);
        TaskId = taskId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientConcurrentModel"/> class with the specified model and endpoint.
    /// </summary>
    /// <param name="model">The model to clone.</param>
    /// <param name="endPoint">The endpoint.</param>
    public HttpClientConcurrentModel(HttpClientConcurrentModel model, string endPoint) : base(model.TaskId)
    {
        StatusCall = model.StatusCall;
        TaskId = model.TaskId;
        TaskCount = model.TaskCount;
        DurationMS = model.DurationMS;
        SemaphoreCount = model.SemaphoreCount;
        SemaphoreWaitTicks = model.SemaphoreWaitTicks;
    }

    /// <summary>
    /// Gets or sets the status call for sending HTTP requests.
    /// </summary>
    public HttpClientSendRequest<SiteStatus> StatusCall { get; set; } = default!;
}
