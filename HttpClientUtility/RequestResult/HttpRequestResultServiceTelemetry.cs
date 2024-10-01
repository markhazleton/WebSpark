using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientUtility.RequestResult;


/// <summary>
/// Class HttpRequestResultServiceTelemetry adds telemetry to the IHttpRequestResultService implementation
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpRequestResultServiceTelemetry"/> class
/// </remarks>
/// <param name="logger">ILogger instance</param>
/// <param name="service">IHttpRequestResultService instance</param>
public class HttpRequestResultServiceTelemetry(ILogger<HttpRequestResultServiceTelemetry> logger, IHttpRequestResultService service) : IHttpRequestResultService
{

    /// <summary>
    /// GetAsync performs a GET request and adds telemetry information to the response.
    /// </summary>
    /// <typeparam name="T">Result type of the GET request</typeparam>
    /// <param name="statusCall">HttpRequestResult instance</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>HttpRequestResult instance including telemetry information</returns>
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> statusCall, CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.HttpSendRequestResultAsync(statusCall, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall.ErrorList.Add($"Telemetry:GetAsync:Exception:{ex.Message}");
            logger.LogCritical("Telemetry:GetAsync:Exception:{Message}", ex.Message);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.UtcNow;
        return statusCall;
    }
}
