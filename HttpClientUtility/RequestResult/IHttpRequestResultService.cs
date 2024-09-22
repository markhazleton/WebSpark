namespace HttpClientUtility.RequestResult;
/// <summary>
/// HttpClientService interface to send HTTP requests.
/// </summary>
public interface IHttpRequestResultService
{
    /// <summary>
    /// HttpSendRequestAsync
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCall"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<HttpRequestResult<T>> HttpSendRequestAsync<T>(HttpRequestResult<T> statusCall, CancellationToken ct);
}
