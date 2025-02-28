using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HttpClientUtility.RequestResult;


/// <summary>
/// Class HttpRequestResultServiceTelemetry adds telemetry to the IHttpRequestResultService implementation
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpRequestResultServiceTelemetry"/> class
/// </remarks>
/// <param name="logger">ILogger instance</param>
/// <param name="service">IHttpRequestResultService instance</param>
public class HttpRequestResultServiceTelemetry(ILogger<HttpRequestResultServiceTelemetry> logger, 
    IHttpRequestResultService service) : IHttpRequestResultService
{
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> statusCall,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default)
    {
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.HttpSendRequestResultAsync(statusCall,
                memberName, filePath, lineNumber,ct).ConfigureAwait(false);
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
