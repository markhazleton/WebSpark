using HttpClientUtility.MockService;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Diagnostics;
using System.Net;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

/// <summary>
/// Controller for demonstrating the use of Polly for handling retries in HTTP requests.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PollyController"/> class.
/// </remarks>
/// <param name="logger">The logger instance for logging information.</param>
public class PollyController(
    ILogger<PollyController> logger,
    IHttpClientFactory clientFactory) : AsyncSparkBaseController
{
    private AsyncRetryPolicy<HttpResponseMessage> GetAsyncRetryPolicy(Random Jitter, string RetryCountKey)
    {

        // Initialize Polly retry policy with quick retries for demo purposes and jitter
        var _httpIndexPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromMilliseconds(100) + TimeSpan.FromMilliseconds(Jitter.Next(0, 100)),
                onRetry: (response, timespan, retryCount, context) =>
                {
                    context[RetryCountKey] = retryCount;
                    var message = response.Result?.StatusCode.ToString() ?? "Request failed without response.";
                    if (response.Exception != null)
                    {
                        message += $" Exception: {response.Exception.Message}";
                    }

                    if (context.TryGetValue("ResultsList", out var resultsList))
                    {
                        var results = resultsList as List<string>;
                        results?.Add($"Retry {retryCount}: {message}");
                    }

                    logger.LogWarning("Request failed with {StatusCode}. Waiting {Timespan} before next retry. Retry attempt {RetryCount}.",
                                        response.Result?.StatusCode, timespan, retryCount);
                });
        return _httpIndexPolicy;
    }

    private AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakPolicy()
    {
        // Circuit breaker to open after 3 consecutive failed attempts and reset after 10 seconds
        var _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(10),
                onBreak: (outcome, breakDelay) =>
                {
                    logger.LogWarning("Circuit breaker opened due to {StatusCode}. Waiting {BreakDelay} before next attempt.",
                                       outcome.Result?.StatusCode, breakDelay);
                },
                onReset: () => logger.LogInformation("Circuit breaker reset."),
                onHalfOpen: () => logger.LogInformation("Circuit breaker half-open: Testing the service again."));
        return _circuitBreakerPolicy;
    }

    /// <summary>
    /// Handles the GET request to the home page. Executes an HTTP POST request with retry logic.
    /// </summary>
    /// <param name="loopCount">The number of iterations to perform in the mock operation.</param>
    /// <param name="maxTimeMs">The maximum time allowed for the operation, in milliseconds.</param>
    /// <returns>Returns the result of the operation.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(int loopCount = 1, int maxTimeMs = 1000)
    {
        string RetryCountKey = "retrycount";
        Stopwatch StopWatch = new();
        Random Jitter = new();

        // Start timing the operation
        StopWatch.Reset();
        StopWatch.Start();

        using var cts = new CancellationTokenSource(maxTimeMs); // Apply handling for the cancellation token source
        var context = new Context { { RetryCountKey, 0 }, { "ResultsList", new List<string>() } };
        MockResults mockResults = new() { LoopCount = loopCount, MaxTimeMS = maxTimeMs };
        HttpResponseMessage response = new(HttpStatusCode.InternalServerError);
        try
        {
            var _circuitBreakerPolicy = GetCircuitBreakPolicy();
            var _httpIndexPolicy = GetAsyncRetryPolicy(Jitter, RetryCountKey);
            var BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/");
            var _httpClient = clientFactory.CreateClient("PollyController");

            // Create the HTTP request message
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseAddress}api/AsyncSpark/remote/results")
            {
                Content = JsonContent.Create(mockResults)
            };

            // Execute the HTTP request with retry and circuit breaker logic
            response = await _circuitBreakerPolicy.ExecuteAsync(ctx =>
                _httpIndexPolicy.ExecuteAsync(innerCtx =>
                    _httpClient.SendAsync(request, cts.Token), ctx), context);

            if (response.IsSuccessStatusCode)
            {
                mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
            }
            else
            {
                // Specific exception handling with fallback to general exception
                try
                {
                    mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "An HTTP request error occurred while reading the response content.");
                    mockResults.Message = $"HTTP Error: {ex.Message}";
                }
                catch (TaskCanceledException ex)
                {
                    logger.LogError(ex, "The operation was canceled, likely due to a timeout.");
                    mockResults.Message = $"Timeout Error: {ex.Message}";
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while reading the response content.");
                    mockResults.Message = $"Error: {ex.Message}";
                }
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "An HTTP request error occurred while executing the request.");
            mockResults.Message = $"HTTP Error: {ex.Message}";
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "The operation was canceled, likely due to a timeout.");
            mockResults.Message = $"Timeout Error: {ex.Message}";
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogError(ex, "The circuit is open; requests are not being sent.");
            mockResults.Message = $"Circuit Breaker Open: {ex.Message}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing the HTTP request.");
            mockResults.Message = $"Error: {ex.Message}";
        }

        // Stop timing the operation
        StopWatch.Stop();
        mockResults.RunTimeMS = StopWatch.ElapsedMilliseconds;

        // Retrieve the list of results and store them in the mockResults message
        if (mockResults.Message != "Task Complete")
        {
            if (context.TryGetValue("ResultsList", out var resultsList))
            {
                var results = resultsList as List<string>;
                mockResults.Message += "<hr/>" + string.Join(";<br/> ", results);
            }
        }

        // Return the results to the view
        return View("Index", mockResults);
    }
}
