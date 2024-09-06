using HttpClientUtility.GetService;
using WebSpark.Core.Models;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

/// <summary>
/// Controller for handling bulk HTTP GET calls.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BulkCallsController"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
/// <param name="getCallService">The HTTP GET call service.</param>
public class BulkCallsController(
    ILogger<BulkCallsController> logger, 
    IHttpGetCallService getCallService) : AsyncSparkBaseController
{
    private static readonly object WriteLock = new();

    /// <summary>
    /// Calls the specified endpoint multiple times asynchronously.
    /// </summary>
    /// <param name="maxThreads">The maximum number of concurrent threads.</param>
    /// <param name="iterationCount">The number of iterations.</param>
    /// <param name="endpoint">The endpoint URL.</param>
    /// <returns>A list of HTTP GET call results.</returns>
    private async Task<List<HttpGetCallResults>> CallEndpointMultipleTimes(
        int maxThreads = 1, 
        int iterationCount = 10, 
        string endpoint = "/api/asyncspark/status")
    {
        // check if endpoint is partially specified add current request path
        if (!endpoint.StartsWith("http"))
        {
            endpoint = $"{Request.Scheme}://{Request.Host}{endpoint}";
        }
        int curIndex = 0;
        // Create a SemaphoreSlim with a maximum of maxThreads concurrent requests
        SemaphoreSlim semaphore = new(maxThreads);
        List<HttpGetCallResults> results = [];
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Create a list of tasks to make the GetAsync calls
        List<Task> tasks = [];
        for (int i = 0; i < iterationCount; i++)
        {
            // Acquire the semaphore before making the request
            await semaphore.WaitAsync();
            curIndex++;
            var statusCall = new HttpGetCallResults(curIndex, endpoint);
            // Create a task to make the request
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Get the async results
                    var result = await getCallService.GetAsync<ApplicationStatus>(statusCall, cts.Token);
                    lock (WriteLock)
                    {
                        results.Add(result);
                    }
                }
                finally
                {
                    // Release the semaphore
                    semaphore.Release();
                }
            }));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Log a message when all calls are complete
        logger.LogInformation("All calls complete");
        return results;
    }

    // GET: BulkCallsController
    /// <summary>
    /// Action method for the index page.
    /// </summary>
    /// <returns>The index view with the results of the bulk HTTP GET calls.</returns>
    public async Task<ActionResult> Index()
    {
        var results = await CallEndpointMultipleTimes();
        return View(results);
    }
}
