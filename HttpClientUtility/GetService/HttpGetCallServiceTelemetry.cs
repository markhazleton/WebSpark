using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientUtility.GetService;

public class HttpGetCallServiceTelemetry : IHttpGetCallService
{
    private readonly ILogger<HttpGetCallServiceTelemetry> _logger;
    private readonly IHttpGetCallService _service;

    public HttpGetCallServiceTelemetry(ILogger<HttpGetCallServiceTelemetry> logger, IHttpGetCallService service)
    {
        _logger = logger;
        _service = service;
    }
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
            _logger.LogCritical("Telemetry:GetAsync:Exception", ex.Message);
        }
        sw.Stop();
        response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        response.CompletionDate = DateTime.Now;
        return response;
    }
}
