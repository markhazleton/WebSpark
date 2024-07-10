using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientUtility.SendService;


/// <summary>
/// Class HttpClientSendServiceTelemetry adds telemetry to the IHttpClientService implementation
/// </summary>
public class HttpClientSendServiceTelemetry : IHttpClientService
{
    private readonly ILogger<HttpClientSendServiceTelemetry> _logger;
    private readonly IHttpClientService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientSendServiceTelemetry"/> class
    /// </summary>
    /// <param name="logger">ILogger instance</param>
    /// <param name="service">IHttpClientService instance</param>
    public HttpClientSendServiceTelemetry(ILogger<HttpClientSendServiceTelemetry> logger, IHttpClientService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// GetAsync performs a GET request and adds telemetry information to the response.
    /// </summary>
    /// <typeparam name="T">Result type of the GET request</typeparam>
    /// <param name="statusCall">HttpClientSendRequest instance</param>
    /// <returns>HttpClientSendRequest instance including telemetry information</returns>
    /// <param name="cts"></param>
    public async Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await _service.HttpClientSendAsync(statusCall, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall.ErrorList.Add($"Telemetry:GetAsync:Exception:{ex.Message}");
            _logger.LogCritical("Telemetry:GetAsync:Exception:{Message}", ex.Message);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }
}
