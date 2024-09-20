using System.Net;

namespace HttpClientUtility.SendService;

public interface IResponseInfo
{
    DateTime? CompletionDate { get; set; }
    long ElapsedMilliseconds { get; set; }
    HttpStatusCode StatusCode { get; set; }
    string ResultAge { get; }
}
