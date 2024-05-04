using HttpClientUtility.Models;
using System.Text.Json.Serialization;

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
public record BuildVersion(
     [property: JsonPropertyName("majorVersion")] int? MajorVersion,
     [property: JsonPropertyName("minorVersion")] int? MinorVersion,
     [property: JsonPropertyName("build")] int? Build,
     [property: JsonPropertyName("revision")] int? Revision
 );
public record Tests();
public record Features();
public record SiteStatus(
    [property: JsonPropertyName("buildDate")] DateTime? BuildDate,
    [property: JsonPropertyName("buildVersion")] BuildVersion BuildVersion,
    [property: JsonPropertyName("features")] Features Features,
    [property: JsonPropertyName("messages")] IReadOnlyList<object> Messages,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("status")] int? Status,
    [property: JsonPropertyName("tests")] Tests Tests
);
