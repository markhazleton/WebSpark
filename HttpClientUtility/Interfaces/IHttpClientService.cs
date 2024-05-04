using HttpClientUtility.Models;

namespace HttpClientUtility.Interfaces;

public interface IHttpClientService
{
    Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct);
}
