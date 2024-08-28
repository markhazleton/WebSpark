namespace OpenWeatherMapClient.WeatherService;

public class WeatherServiceLoggingDecorator : IOpenWeatherMapClient
{
    public WeatherServiceLoggingDecorator(IOpenWeatherMapClient weatherService, ILogger<WeatherServiceLoggingDecorator> logger)
    {
        _innerWeatherService = weatherService;
        _logger = logger;
    }
    private IOpenWeatherMapClient _innerWeatherService;
    private ILogger<WeatherServiceLoggingDecorator> _logger;

    public async Task<CurrentWeather> GetCurrentWeatherAsync(string location)
    {
        var sw = Stopwatch.StartNew();
        var currentWeather = await _innerWeatherService.GetCurrentWeatherAsync(location);
        sw.Stop();
        var elapsedMS = sw.ElapsedMilliseconds;
        _logger.LogWarning("Retrieved weather data for {location} - Elapsed ms: {elapsedMS} {currentWeather}", location, elapsedMS, currentWeather?.Location?.Name);
        return currentWeather ?? new CurrentWeather();
    }
    public async Task<LocationForecast> GetForecastAsync(string location)
    {
        return await _innerWeatherService.GetForecastAsync(location);
    }
}
