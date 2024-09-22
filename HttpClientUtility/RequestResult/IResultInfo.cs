using System.Net;

namespace HttpClientUtility.RequestResult;

public interface IResultInfo
{
    DateTime? CompletionDate { get; set; }
    long ElapsedMilliseconds { get; set; }
    HttpStatusCode StatusCode { get; set; }
    string ResultAge { get; }
}
