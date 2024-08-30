using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HttpClientUtility.GetService;

public class HttpGetCallService : IHttpGetCallService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<HttpGetCallService> _logger;

    public HttpGetCallService(ILogger<HttpGetCallService> logger, IHttpClientFactory httpClientFactory)
    {
        _clientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task<HttpGetCallResults> GetAsync<T>(HttpGetCallResults statusCall, CancellationToken ct)
    {
        try
        {
            using var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.GetAsync(statusCall.StatusPath, ct);
            response.EnsureSuccessStatusCode();
            var StatusResults = await response.Content.ReadAsStringAsync(ct);
            try
            {
                statusCall.StatusResults = JsonSerializer.Deserialize<T>(StatusResults);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("HttpGetCallService:GetAsync:DeserializeException", ex.Message);
                statusCall.StatusResults = JsonSerializer.Deserialize<dynamic>(StatusResults);
            }

        }
        catch (Exception ex)
        {
            _logger.LogCritical("HttpGetCallService:GetAsync:Exception", ex.Message);
        }
        return statusCall;
    }
}
