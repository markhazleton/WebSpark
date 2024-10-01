namespace HttpClientUtility.RequestResult;
/// <summary>
/// HttpClientService interface to send HTTP requests.
/// </summary>
public interface IHttpRequestResultService
{
    /// <summary>
    /// HttpSendRequestResultAsync
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCall"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> statusCall, CancellationToken ct);
}
