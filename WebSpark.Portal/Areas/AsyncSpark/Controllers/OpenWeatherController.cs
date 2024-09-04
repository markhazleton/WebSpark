using HttpClientUtility.MemoryCache;
using OpenWeatherMapClient.Interfaces;
using OpenWeatherMapClient.Models;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

/// <summary>
/// OpenWeatherController
/// </summary>
/// <param name="_logger"></param>
/// <param name="_weatherClient"></param>
/// <param name="_cacheManager"></param>
public class OpenWeatherController(
    ILogger<HomeController> _logger,
    IOpenWeatherMapClient _weatherClient,
    IMemoryCacheManager _cacheManager) : AsyncSparkBaseController
{
    private const string CurrentWeatherListCacheKey = "CurrentWeatherList";

    private List<CurrentWeather> AddCurrentWeatherList(CurrentWeather currentWeather)
    {
        var theList = GetCurrentWeatherList();
        if (!theList.Any(w => w.Location?.Name == currentWeather?.Location?.Name))
        {
            theList.Add(currentWeather);
            _cacheManager.Set(CurrentWeatherListCacheKey, theList, 30); // Cache time in minutes
        }
        return theList;
    }

    private List<CurrentWeather> GetCurrentWeatherList()
    {
        return _cacheManager.Get(CurrentWeatherListCacheKey, () =>
        {
            return new List<CurrentWeather>();
        }, 30); // Cache time in minutes
    }

    /// <summary>
    /// Handles fetching and caching current weather information.
    /// </summary>
    /// <param name="location">Optional location to fetch weather data for. Defaults to "Dallas" if null.</param>
    /// <returns>An IActionResult representing the view of current weather data.</returns>
    public async Task<IActionResult> Index(string? location = null)
    {
        location ??= "Dallas";  // Default location if none provided
        var myList = GetCurrentWeatherList();

        if (myList.Any(w => w.Location?.Name == location))
        {
            _logger.LogInformation("Location {location} is already in the list", location);
        }
        else
        {
            var conditions = await _weatherClient.GetCurrentWeatherAsync(location);
            if (!conditions.Success)
            {
                conditions.ErrorMessage = $"{conditions.ErrorMessage} received for location: {location}";
                _logger.LogError(conditions.ErrorMessage);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved weather data for {location}", location);
            }
            myList = AddCurrentWeatherList(conditions);
        }
        return View(myList);
    }
}
