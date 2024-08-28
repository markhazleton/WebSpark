using AsyncSpark.HttpGetCall;
using Microsoft.Extensions.Caching.Memory;
using OpenWeatherMapClient.Interfaces;
using OpenWeatherMapClient.Models;
using WebSpark.Core.Models;

namespace WebSpark.Main.Areas.Async.Controllers;




/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="logger"></param>
/// <param name="weatherService"></param>
/// <param name="cache"></param>
public class OpenWeatherController(ILogger<HomeController> logger, IOpenWeatherMapClient weatherService, IMemoryCache cache) : AsyncBaseController
{
    private const string LocationCacheKey = "LocationCacheKey";
    private readonly ILogger<HomeController> _logger = logger;

    private string VerifyLocation(string? location)
    {
        string? theLocation = location;
        if (location is null)
        {
            if (cache.TryGetValue(LocationCacheKey, out theLocation))
            {
            }
            else
            {
                theLocation = "Dallas";
            }
        }
        cache.Set(LocationCacheKey, theLocation ?? "Dallas", TimeSpan.FromMinutes(30));
        return theLocation ?? "Dallas";
    }

    private List<CurrentWeather> GetCurrentWeatherList()
    {
        List<CurrentWeather>? theList;
        if (cache.TryGetValue("CurrentWeatherList", out theList))
        {

        }
        else
        {
            theList = [];
        }
        cache.Set("CurrentWeatherList", theList ?? [], TimeSpan.FromMinutes(30));
        return theList ?? [];
    }
    private List<CurrentWeather> AddCurrentWeatherList(CurrentWeather currentWeather)
    {
        List<CurrentWeather>? theList;
        if (cache.TryGetValue("CurrentWeatherList", out theList))
        {
        }
        else
        {
            theList = [];
        }
        if (theList.Where(w => w.Location?.Name == currentWeather?.Location?.Name).Any())
        {
            // Location is already in list
        }
        else
        {
            theList.Add(currentWeather);
        }
        cache.Set("CurrentWeatherList", theList ?? [], TimeSpan.FromMinutes(30));
        return theList ?? [];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public async Task<IActionResult> Index(string? location = null)
    {
        var myList = GetCurrentWeatherList();
        string theLocation = VerifyLocation(location);

        if (myList.Where(w => w.Location?.Name == theLocation).Any())
        {
            _logger.LogInformation("Location {theLocation} is already in the list", theLocation);
        }
        else
        {
            CurrentWeather conditions = await weatherService.GetCurrentWeatherAsync(theLocation);
            if (!conditions.Success)
            {
                conditions.ErrorMessage = $"{conditions.ErrorMessage} received for location:{theLocation}";
                _logger.LogError(conditions.ErrorMessage);
            }
            myList = AddCurrentWeatherList(conditions);
        }
        return View(myList);
    }



}
