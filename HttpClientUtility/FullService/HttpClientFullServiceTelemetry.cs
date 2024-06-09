using HttpClientUtility.Models;
using System.Diagnostics;
using System.Net;

namespace HttpClientUtility.FullService
{
    public class HttpClientFullServiceTelemetry(IHttpClientFullService service) : IHttpClientFullService
    {
        public HttpClient CreateConfiguredClient()
        {
            return service.CreateConfiguredClient();
        }

        public Task<HttpResponseContent<TResult>> DeleteAsync<TResult>(Uri requestUri, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseContent<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

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
}
