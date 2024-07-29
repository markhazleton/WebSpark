namespace HttpClientUtility.SendService;
/// <summary>
/// HttpClientService interface to send HTTP requests.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// HttpClientSendAsync
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCall"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct);
}
