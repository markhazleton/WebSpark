using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientUtility.GetService;

/// <summary>
/// Handles telemetry for an HTTP GET call.
/// </summary>
public class HttpGetCallServiceTelemetry : IHttpGetCallService
{
    private readonly ILogger<HttpGetCallServiceTelemetry> _logger;
    private readonly IHttpGetCallService _service;

    /// <summary>
    /// Constructs a new instance 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="service"></param>
    public HttpGetCallServiceTelemetry(ILogger<HttpGetCallServiceTelemetry> logger, IHttpGetCallService service)
    {
        _logger = logger;
        _service = service;
    }
    /// <summary>
    /// Get Async
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCall"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<HttpGetCallResults> GetAsync<T>(HttpGetCallResults statusCall, CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();
        var response = new HttpGetCallResults(statusCall);
        try
        {
            response = await _service.GetAsync<T>(statusCall, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetAsync for StatusCall.Path: {StatusPath}", statusCall.StatusPath);
        }
        sw.Stop();
        response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        response.CompletionDate = DateTime.Now;
        return response;
    }
}
