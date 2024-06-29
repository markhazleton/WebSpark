using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TriviaSpark.Core.OpenTriviaDb;

/// <summary>
/// Class HttpGetCallServiceTelemetry adds telemetry to the IHttpGetCallService implementation
/// </summary>
public class HttpGetCallServiceTelemetry : IHttpGetCallService
{
    private readonly ILogger<HttpGetCallServiceTelemetry> _logger;
    private readonly IHttpGetCallService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpGetCallServiceTelemetry"/> class
    /// </summary>
    /// <param name="logger">ILogger instance</param>
    /// <param name="service">IHttpGetCallService instance</param>
    public HttpGetCallServiceTelemetry(ILogger<HttpGetCallServiceTelemetry> logger, IHttpGetCallService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// GetAsync performs a GET request and adds telemetry information to the response.
    /// </summary>
    /// <typeparam name="T">Result type of the GET request</typeparam>
    /// <param name="statusCall">HttpGetCallResults instance</param>
    /// <returns>HttpGetCallResults instance including telemetry information</returns>
    /// <param name="cts"></param>
    public async Task<HttpGetCallResults<T>> GetAsync<T>(HttpGetCallResults<T> statusCall, CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();
        var response = new HttpGetCallResults<T>(statusCall);
        try
        {
            response = await _service.GetAsync(statusCall, ct);
        }
        catch (Exception ex)
        {
            response.ErrorMessage = $"Telemetry:GetAsync:Exception:{ex.Message}";
            _logger.LogCritical("Telemetry:GetAsync:Exception", ex.Message);
        }
        sw.Stop();
        response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        response.CompletionDate = DateTime.Now;
        return response;
    }
}
