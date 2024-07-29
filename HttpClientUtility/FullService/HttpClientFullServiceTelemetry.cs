using System.Diagnostics;
using System.Net;

namespace HttpClientUtility.FullService;

/// <summary>
/// Represents a telemetry wrapper for the HttpClientFullService.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpClientFullServiceTelemetry"/> class.
/// </remarks>
/// <param name="service">The underlying HttpClientFullService.</param>
public class HttpClientFullServiceTelemetry(IHttpClientFullService service) : IHttpClientFullService
{

    /// <inheritdoc/>
    public HttpClient CreateConfiguredClient()
    {
        return service.CreateConfiguredClient();
    }

    /// <inheritdoc/>
    public Task<HttpResponseContent<TResult>> DeleteAsync<TResult>(Uri requestUri, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<HttpResponseContent<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        HttpResponseContent<TResult> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.PostAsync<T, TResult>(requestUri, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<TResult>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.Now;
        return statusCall;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        HttpResponseContent<TResult> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.PostAsync<T, TResult>(requestUri, payload, headers, cancellationToken);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<TResult>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.Now;
        return statusCall;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PutAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        HttpResponseContent<TResult> statusCall;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await service.PutAsync<T, TResult>(requestUri, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            statusCall = HttpResponseContent<TResult>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.Now;
        return statusCall;
    }
}
