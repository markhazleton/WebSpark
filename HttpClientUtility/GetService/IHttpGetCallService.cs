
namespace HttpClientUtility.GetService;

public interface IHttpGetCallService
{
    Task<HttpGetCallResults> GetAsync<T>(HttpGetCallResults statusCall, CancellationToken ct);
}
