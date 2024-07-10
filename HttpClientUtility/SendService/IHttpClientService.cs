namespace HttpClientUtility.SendService;

public interface IHttpClientService
{
    Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct);
}
