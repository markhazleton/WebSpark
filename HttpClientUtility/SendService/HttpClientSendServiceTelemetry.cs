using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientUtility.SendService;


/// <summary>
/// Class HttpClientSendServiceTelemetry adds telemetry to the IHttpClientSendService implementation
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpClientSendServiceTelemetry"/> class
/// </remarks>
/// <param name="logger">ILogger instance</param>
/// <param name="service">IHttpClientSendService instance</param>
public class HttpClientSendServiceTelemetry(ILogger<HttpClientSendServiceTelemetry> logger, IHttpClientSendService service) : IHttpClientSendService
{

    /// <summary>
    /// GetAsync performs a GET request and adds telemetry information to the response.
    /// </summary>
    /// <typeparam name="T">Result type of the GET request</typeparam>
    /// <param name="statusCall">HttpClientSendRequest instance</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>HttpClientSendRequest instance including telemetry information</returns>
    public async Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.HttpClientSendAsync(statusCall, ct).ConfigureAwait(false);
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
