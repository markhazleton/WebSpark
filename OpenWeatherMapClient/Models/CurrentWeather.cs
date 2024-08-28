
namespace OpenWeatherMapClient.Models;

public class CurrentWeather
{
    public TimeSpan ObservationAge
    {
        get { return DateTime.Now.Subtract(FetchTime); }
    }
    public WeatherData? CurrentConditions { get; set; }
    public String? ErrorMessage { get; set; }
    public DateTime FetchTime { get; set; }
    public LocationData? Location { get; set; }
    public DateTime ObservationTime { get; set; }
    public DateTime ObservationTimeUtc { get; set; }
    public bool Success { get; set; }

    public class LocationData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public String? Name { get; set; }
    }

    public class WeatherData
    {
        public int CloudCover { get; set; }
        public String? Conditions { get; set; }
        public String? ConditionsDescription { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double RainfallOneHour { get; set; }
        public double Temperature { get; set; }
        public double Visibility { get; set; }
        public CompassDirection? WindDirection { get; set; }
        public double WindDirectionDegrees { get; set; }
        public double WindSpeed { get; set; }
    }
}
