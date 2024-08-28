
namespace OpenWeatherMapClient.Models;

public class WeatherForecast
{
    public DateTime ForecastTime { get; set; }
    public DateTime ForecastTimeUtc { get; set; }
    public String? Conditions { get; set; }
    public String? ConditionsDescription { get; set; }
    public int CloudCover { get; set; }
    public double Temperature { get; set; }
    public double Pressure { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public CompassDirection? WindDirection { get; set; }
    public double WindDirectionDegrees { get; set; }
    public double ExpectedRainfall { get; set; }
    public double ExpectedSnowfall { get; set; }
}
