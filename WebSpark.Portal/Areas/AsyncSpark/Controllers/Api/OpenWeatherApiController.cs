using OpenWeatherMapClient.Interfaces;
using WebSpark.HttpClientUtility.MemoryCache;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers.Api;

/// <summary>
/// Controller for handling OpenWeather API requests.
/// </summary>
[Route("api/[area]/[controller]")]
public class OpenWeatherApiController(
    ILogger<OpenWeatherApiController> logger,
    IOpenWeatherMapClient weatherClient,
    IMemoryCacheManager cacheManager) : BaseAsyncSparkApiController
{
    private const string RateLimitCacheKey = "WeatherRateLimit";
    private const int RequestLimit = 5; // Limit of 5 requests per minute
    private readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Checks if the request rate limit is exceeded.
    /// </summary>
    private bool IsRateLimitExceeded()
    {
        var requestTimestamps = cacheManager.Get(RateLimitCacheKey, () => new List<DateTime>());
        var now = DateTime.UtcNow;
        // Remove old timestamps that are outside the rate limit window
        requestTimestamps.RemoveAll(t => now - t > RateLimitWindow);
        if (requestTimestamps.Count >= RequestLimit)
        {
            return true;
        }
        // Add the current timestamp and update the cache
        requestTimestamps.Add(now);
        cacheManager.Set(RateLimitCacheKey, requestTimestamps, RequestLimit);
        return false;
    }

    /// <summary>
    /// API endpoint to get the current weather for a specific location.
    /// </summary>
    /// <param name="location">Location to fetch weather data for.</param>
    /// <returns>Weather data in JSON format or a rate limit message.</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [HttpGet("weather")]
    public async Task<IActionResult> GetWeather(string location)
    {
        // Check if rate limit is exceeded
        if (IsRateLimitExceeded())
        {
            var errorMessage = "Rate limit exceeded. Please try again later.";
            logger.LogError(errorMessage);
            return StatusCode(429, errorMessage); // 429 Too Many Requests
        }

        var conditions = await weatherClient.GetCurrentWeatherAsync(location);
        if (!conditions.Success)
        {
            var errorMessage = $"{conditions.ErrorMessage} received for location: {location}";
            logger.LogError("Error: {ErrorMessage}", errorMessage);
            return BadRequest(errorMessage);
        }
        return Ok(conditions); // Return weather data as JSON
    }
}

