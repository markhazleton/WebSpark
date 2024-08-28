using AsyncSpark.Models;
using Polly;
using Polly.Retry;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace WebSpark.Main.Areas.Async.Controllers;



/// <summary>
/// Controller for demonstrating the use of Polly for handling retries in HTTP requests.
/// </summary>
public class PollyController : AsyncBaseController
{
    private readonly ILogger<PollyController> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _httpIndexPolicy;
    private const string RetryCountKey = "retrycount";
    private readonly HttpClient _httpClient;
    private static readonly Stopwatch StopWatch = new();
    private static readonly Random Jitter = new();
    private readonly CancellationTokenSource Cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PollyController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    public PollyController(ILogger<PollyController> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        Cts = new CancellationTokenSource();

        // Initialize HttpClient and set default request headers
        _httpClient = clientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Initialize Polly retry policy with exponential backoff and jitter
        _httpIndexPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(1, retryAttempt) / 2)
                                + TimeSpan.FromSeconds(Jitter.Next(0, 1)),
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

                    _logger.LogWarning($"Request failed with {response.Result?.StatusCode}. Waiting {timespan} before next retry. Retry attempt {retryCount}.");
                });
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
        // Start timing the operation
        StopWatch.Reset();
        StopWatch.Start();

        // Set the base address for the HttpClient
        _httpClient.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/");

        var context = new Context { { RetryCountKey, 0 }, { "ResultsList", new List<string>() } };
        var mockResults = new MockResults { LoopCount = loopCount, MaxTimeMS = maxTimeMs };
        HttpResponseMessage response = new(HttpStatusCode.InternalServerError);

        try
        {
            // Execute the HTTP request with retry logic
            response = await _httpIndexPolicy.ExecuteAsync(ctx =>
                HttpClientJsonExtensions.PostAsJsonAsync(_httpClient, "remote/Results", mockResults, Cts.Token), context);

            if (response.IsSuccessStatusCode)
            {
                mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
            }
            else
            {
                // wrap in try/catch to handle exceptions when reading the response content
                try
                {
                    mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while reading the response content.");
                    mockResults.Message = $"Error: {ex.Message}";
                }

                mockResults.ResultValue = response.StatusCode.ToString();

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the HTTP request.");
            mockResults.Message = $"Error: {ex.Message}";
        }

        // Stop timing the operation
        StopWatch.Stop();
        mockResults.RunTimeMS = StopWatch.ElapsedMilliseconds;

        // Retrieve the list of results and store them in the mockResults message
        if (context.TryGetValue("ResultsList", out var resultsList))
        {
            var results = resultsList as List<string>;
            mockResults.Message += "<hr/>" + string.Join(";<br/> ", results);
        }

        // Return the results to the view
        return View("Index", mockResults);
    }
}