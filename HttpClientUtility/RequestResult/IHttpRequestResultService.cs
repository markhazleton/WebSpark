using System.Runtime.CompilerServices;

namespace HttpClientUtility.RequestResult;
/// <summary>
/// HttpClientService interface to send HTTP requests.
/// </summary>
public interface IHttpRequestResultService
{

    Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> httpSendResults,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default);
}
