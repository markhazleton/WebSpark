
namespace OpenWeatherMapClient.Models;
public class LocationForecast
{
    public LocationForecast()
    {
        Forecasts = [];
    }
    public bool Success { get; set; }
    public String? ErrorMessage { get; set; }
    public ForecastLocation? Location { get; set; }
    public DateTime FetchTime { get; set; }
    public List<WeatherForecast> Forecasts { get; private set; }
}
