
namespace OpenWeatherMapClient.Services;

/// <summary>
/// Class ClientBase.
/// Implements the <see cref="System.IDisposable" />
/// </summary>
/// <seealso cref="System.IDisposable" />
public abstract class RestClientBase : IDisposable, IRestClientBase
{
    /// <summary>
    /// Lazy Client used to instantiate when needed rather than during constructor
    /// </summary>
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// ClientBase constructor used to set Application Name and Base Url for requests
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="appName">Name of the application.</param>
    protected RestClientBase(string baseUrl, string appName, IHttpClientFactory clientFactory, ILogger logger)
    {
        if (string.IsNullOrEmpty(appName))
            baseUrl = "MISSING";

        AppName = appName;
        BaseAPIUrl = baseUrl.Trim('/');
        _clientFactory = clientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Client Base Destructor part of the IDisposable implementation
    /// </summary>
    ~RestClientBase() { Dispose(false); }


    /// <summary>
    /// Gets the HttpClient for this class,a lazy pattern is used to create an instance when needed but never more
    /// than a single instance
    /// </summary>
    /// <returns>RestClient.</returns>
    /// <exception cref="ObjectDisposedException">RestClient has been disposed</exception>
    protected HttpClient Client()
    {
        if (_clientFactory == null)
            throw new ObjectDisposedException("Client Factory has been disposed");
        return _clientFactory.CreateClient();
    }

    /// <summary>
    /// Client Base Dispose Property part of the IDisposable implementation
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        // There are no unmanaged resources to release, but
        // if we add them, they need to be released here.
    }

    /// <summary>
    /// Puts the specified URL segment.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="urlSegment">The URL segment.</param>
    /// <param name="requestBody">The request body.</param>
    /// <returns>T.</returns>
    protected async Task<T> ExecuteAsync<T>(HttpRequestMessage request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var response = await Client().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(true);

        if (response.IsSuccessStatusCode)
        {
            // perhaps check some headers before deserializing
            try
            {
                var content = await response.Content.ReadAsStringAsync(token).ConfigureAwait(true);
                var contentConvert = JsonConvert.DeserializeObject<T>(content);
                return contentConvert;
            }
            catch (NotSupportedException) // When content type is not valid
            {
                IsError = true;
                Status = "The content type is not supported.";
                _logger.LogError(Status);
            }
            catch (JsonException) // Invalid JSON
            {
                IsError = true;
                Status = "Invalid JSON.";
                _logger.LogError(Status);
            }
        }
        else
        {
            _logger.LogError("Bad Response({}) from {}:{}", response.StatusCode, response.RequestMessage.RequestUri, response);
            IsError = true;
            Status = response?.ReasonPhrase?.ToString() ?? string.Empty;
        }
        return default;
    }


    /// <summary>
    /// Client Base Dispose Property part of the IDisposable implementation
    /// </summary>
    public void Dispose()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    public HttpRequestMessage GetRequest(string urlSegment, HttpMethod method)
    {
        var request = new HttpRequestMessage(method, new Uri($"{BaseAPIUrl}{urlSegment}"));
        return request;
    }

    /// <summary>
    /// The Name of the Client Application making the API Call
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// The Root of the Domain with the API
    /// </summary>
    public string BaseAPIUrl { get; set; } = string.Empty;

    /// <summary>
    /// Public Flag for when the API Encounters and Error
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// The Status of the last request made from the API Client
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The User ID that is used to make the request, this defaults to Web Account
    /// </summary>
    public int UserID { get; set; }
}
