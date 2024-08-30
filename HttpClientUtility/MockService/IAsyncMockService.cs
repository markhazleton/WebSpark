
namespace HttpClientUtility.MockService;

public interface IAsyncMockService
{
    Task<decimal> LongRunningOperation(int loop);
    Task<decimal> LongRunningOperationWithCancellationTokenAsync(int loop, CancellationToken cancellationToken);
}
