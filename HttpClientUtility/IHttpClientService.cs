namespace HttpClientUtility;
/// <summary>
/// Interface for HttpClientService
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Returns a configured HttpClient instance from the factory   
    /// </summary>
    /// <returns></returns>
    HttpClient CreateConfiguredClient();
    /// <summary>
    /// Process Http Get request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="requestUri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseContent<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken);
    /// <summary>
    /// Sends an HTTP PUT request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the request payload.</typeparam>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    Task<HttpResponseContent<TResult>> PutAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default);
    /// <summary>
    /// Sends an HTTP POST request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="T">The type of the request payload.</typeparam>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="payload">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default);

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
    Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, Dictionary<string, string> headers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP DELETE request asynchronously and returns the response content.
    /// </summary>
    /// <typeparam name="TResult">The type of the response content.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of HttpResponseContent containing the response content and status code.</returns>
    Task<HttpResponseContent<TResult>> DeleteAsync<TResult>(Uri requestUri, CancellationToken cancellationToken = default);

}