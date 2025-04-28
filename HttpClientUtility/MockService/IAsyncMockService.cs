namespace HttpClientUtility.MockService;

/// <summary>
/// Interface for mock asynchronous operations, primarily used for testing and demonstration purposes.
/// </summary>
public interface IAsyncMockService
{
    /// <summary>
    /// Simulates a long-running operation that returns a decimal value.
    /// </summary>
    /// <param name="loop">The number of iterations to simulate work</param>
    /// <returns>A task that represents the asynchronous operation, containing a decimal result</returns>
    Task<decimal> LongRunningOperation(int loop);

    /// <summary>
    /// Simulates a long-running operation that supports cancellation.
    /// </summary>
    /// <param name="loop">The number of iterations to simulate work</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>A task that represents the asynchronous operation, containing a decimal result</returns>
    Task<decimal> LongRunningOperationWithCancellationTokenAsync(int loop, CancellationToken cancellationToken);
}
