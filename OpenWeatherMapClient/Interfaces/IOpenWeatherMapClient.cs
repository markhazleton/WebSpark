
namespace OpenWeatherMapClient.Interfaces;
public interface IOpenWeatherMapClient
{
    Task<CurrentWeather> GetCurrentWeatherAsync(string location);
    Task<LocationForecast> GetForecastAsync(String location);
}
