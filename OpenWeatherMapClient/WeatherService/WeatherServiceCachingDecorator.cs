﻿namespace OpenWeatherMapClient.WeatherService;

public class WeatherServiceCachingDecorator(
    IOpenWeatherMapClient _weatherService,
    IMemoryCache _cache,
    ILogger<WeatherServiceCachingDecorator> _logger) : IOpenWeatherMapClient
{
    public async Task<CurrentWeather> GetCurrentWeatherAsync(string location)
    {
        string cacheKey = $"WeatherConditions::{location}";
        if (_cache.TryGetValue<CurrentWeather>(cacheKey, out var currentWeather))
        {
            _logger.LogWarning("Retrieved weather data for {location} from cache - {@currentWeather}", location, currentWeather?.Location?.Name);
            return currentWeather ?? new CurrentWeather();
        }
        else
        {
            currentWeather = await _weatherService.GetCurrentWeatherAsync(location);
            if (currentWeather.Success)
            {
                if (location.Equals(currentWeather?.Location?.Name?.ToLower(), StringComparison.CurrentCultureIgnoreCase))
                {
                    _cache.Set(cacheKey, currentWeather, TimeSpan.FromMinutes(90));
                }
                else
                {
                    _logger.LogError("Failed to get weather data for {location}: Error Message: {@currentWeather}", location, currentWeather?.ErrorMessage);
                }
            }
            return currentWeather ?? new CurrentWeather();
        }
    }
    public async Task<LocationForecast> GetForecastAsync(string location)
    {
        string cacheKey = $"WeatherForecast::{location}";
        if (_cache.TryGetValue<LocationForecast>(cacheKey, out var forecast))
        {
            return forecast;
        }
        else
        {
            var locationForecast = await _weatherService.GetForecastAsync(location);
            _cache.Set(cacheKey, locationForecast, TimeSpan.FromMinutes(30));
            return locationForecast;
        }
    }
}
