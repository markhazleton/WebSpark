using HttpClientUtility.StringConverter;
using System.Net;
using System.Text;

namespace HttpClientUtility.ClientService;



/// <summary>
/// Represents a service for making HTTP requests using HttpClient.
/// </summary>
/// <remarks>
/// Initializes a new instance of the HttpClientService class.
/// </remarks>
/// <param name="httpClientFactory">The factory for creating HttpClient instances.</param>
/// <param name="stringConverter">The string converter for serializing and deserializing objects.</param>
/// <exception cref="ArgumentNullException">Thrown when httpClientFactory or stringConverter is null.</exception>
public class HttpClientService(IHttpClientFactory httpClientFactory, IStringConverter stringConverter) : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly IStringConverter _stringConverter = stringConverter ?? throw new ArgumentNullException(nameof(stringConverter));

    /// <summary>
    /// Creates a configured HttpClient instance.
    /// </summary>
    /// <returns>The configured HttpClient instance.</returns>
    public HttpClient CreateConfiguredClient()
    {
        return _httpClientFactory.CreateClient("HttpClientService");
    }

    /// <summary>
    /// Executes an HTTP request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="httpRequest">The function that sends the HTTP request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    private async Task<HttpResponseContent<T>> ExecuteRequestAsync<T>(Func<Task<HttpResponseMessage>> httpRequest, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpRequest();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var deserializedContent = _stringConverter.ConvertFromString<T>(content);
                return HttpResponseContent<T>.Success(deserializedContent, response.StatusCode);
            }
            else
            {
                return HttpResponseContent<T>.Failure($"Error: {response.ReasonPhrase}", response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            return HttpResponseContent<T>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException ex)
        {
            return HttpResponseContent<T>.Failure($"Timeout Exception: {ex.Message}", HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            return HttpResponseContent<T>.Failure($"Unexpected Exception: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    private async Task<HttpResponseContent<TResult>> SendAsync<T, TResult>(HttpMethod method, Uri requestUri, T payload, Dictionary<string, string> headers, CancellationToken cancellationToken)
    {
        using var client = CreateConfiguredClient();
        var jsonPayload = _stringConverter.ConvertFromModel(payload);
        using var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(method, requestUri) { Content = httpContent };
        // add the request headers
        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }
        return await ExecuteRequestAsync<TResult>(() => client.SendAsync(request, cancellationToken), cancellationToken);
    }

    private async Task<HttpResponseContent<TResult>> SendAsync<T, TResult>(HttpMethod method, Uri requestUri, T payload, CancellationToken cancellationToken)
    {
        using var client = CreateConfiguredClient();
        var jsonPayload = _stringConverter.ConvertFromModel(payload);
        using var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(method, requestUri) { Content = httpContent };
        return await ExecuteRequestAsync<TResult>(() => client.SendAsync(request, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP DELETE request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public async Task<HttpResponseContent<TResult>> DeleteAsync<TResult>(Uri requestUri, CancellationToken cancellationToken = default)
    {
        using var client = CreateConfiguredClient();
        return await ExecuteRequestAsync<TResult>(() => client.DeleteAsync(requestUri, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP GET request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public async Task<HttpResponseContent<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateConfiguredClient();
            var response = await client.GetAsync(requestUri, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var deserializedContent = _stringConverter.ConvertFromString<T>(content);
                return HttpResponseContent<T>.Success(deserializedContent, response.StatusCode);
            }
            else
            {
                return HttpResponseContent<T>.Failure($"Error: {response.ReasonPhrase}", response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            return HttpResponseContent<T>.Failure($"HTTP Request Exception: {ex.Message}", HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException ex)
        {
            return HttpResponseContent<T>.Failure($"Timeout Exception: {ex.Message}", HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            return HttpResponseContent<T>.Failure($"Unexpected Exception: {ex.Message}", HttpStatusCode.InternalServerError);
        }
        //        return ExecuteRequestAsync<T>(async () => await client.GetAsync(requestUri, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP POST request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the request payload.</typeparam>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        return SendAsync<T, TResult>(HttpMethod.Post, requestUri, payload, cancellationToken);
    }
    /// <summary>
    /// Post with Request Headers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="requestUri"></param>
    /// <param name="payload"></param>
    /// <param name="headers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        return SendAsync<T, TResult>(HttpMethod.Post, requestUri, payload, headers, cancellationToken);
    }


    /// <summary>
    /// Sends an HTTP PUT request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the request payload.</typeparam>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    public Task<HttpResponseContent<TResult>> PutAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        return SendAsync<T, TResult>(HttpMethod.Put, requestUri, payload, cancellationToken);
    }
}
