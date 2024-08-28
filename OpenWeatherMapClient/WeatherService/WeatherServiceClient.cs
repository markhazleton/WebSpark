
namespace OpenWeatherMapClient.WeatherService;

public class WeatherServiceClient : RestClientBase, IOpenWeatherMapClient
{
    private string _apiKey;
    private readonly ILogger<WeatherServiceClient> _logger;

    public WeatherServiceClient(String apiKey, IHttpClientFactory clientFactory, ILogger<WeatherServiceClient> logger) : base("http://api.openweathermap.org", "OpenWeatherMap", clientFactory, logger)
    {
        _apiKey = apiKey;
        _logger = logger;
    }

    private CurrentWeather MapCurrentConditionsResponse(CurrentConditionsResponse apiResponse)
    {
        if (apiResponse == null) return new CurrentWeather()
        {
            ErrorMessage = "Null received from Open Weather Map API",
            Success = false
        };

        var currentConditions = new CurrentWeather()
        {
            Success = true,
            ErrorMessage = String.Empty,
            Location = new CurrentWeather.LocationData()
            {
                Name = apiResponse.Name,
                Latitude = apiResponse.Coordintates.Latitude,
                Longitude = apiResponse.Coordintates.Longitude
            },
            ObservationTime = DateTimeOffset.FromUnixTimeSeconds(apiResponse.ObservationTime + apiResponse.TimezoneOffset).DateTime,
            ObservationTimeUtc = DateTimeOffset.FromUnixTimeSeconds(apiResponse.ObservationTime).DateTime,
            CurrentConditions = new CurrentWeather.WeatherData()
            {
                Conditions = apiResponse.ObservedConditions.FirstOrDefault()?.Conditions,
                ConditionsDescription = apiResponse.ObservedConditions.FirstOrDefault()?.ConditionsDetail,
                Visibility = apiResponse.Visibility / 1609.0,  // Visibility always comes back in meters, even when imperial requested
                CloudCover = apiResponse.Clouds.CloudCover,
                Temperature = apiResponse.ObservationData.Temperature,
                Humidity = apiResponse.ObservationData.Humidity,
                Pressure = apiResponse.ObservationData.Pressure * 0.0295301,  // Pressure always comes back in millibars, even when imperial requested
                WindSpeed = apiResponse.WindData.Speed,
                WindDirection = CompassDirection.GetDirection(apiResponse.WindData.Degrees),
                WindDirectionDegrees = apiResponse.WindData.Degrees,
                RainfallOneHour = apiResponse.Rain?.RainfallOneHour ?? 0.0
            },
            FetchTime = DateTime.Now
        };

        return currentConditions;
    }


    private LocationForecast MapForecastResponse(ForecastResponse openWeatherApiResponse)
    {
        var locationForecast = new LocationForecast()
        {
            Success = true,
            ErrorMessage = String.Empty,
            Location = new ForecastLocation()
            {
                Name = openWeatherApiResponse.Location.Name,
                Latitude = openWeatherApiResponse.Location.Coordinates.Latitude,
                Longitude = openWeatherApiResponse.Location.Coordinates.Longitude
            },
            FetchTime = DateTime.Now
        };

        foreach (var openWeatherForecast in openWeatherApiResponse.ForecastPoints)
        {
            WeatherForecast forecast = new()
            {
                Conditions = openWeatherForecast.Conditions.FirstOrDefault()?.main,
                ConditionsDescription = openWeatherForecast.Conditions.FirstOrDefault()?.description,
                Temperature = openWeatherForecast.WeatherData.Temperature,
                Humidity = openWeatherForecast.WeatherData.Humidity,
                Pressure = openWeatherForecast.WeatherData.pressure * 0.0295301,  // Pressure always comes back in millibars, even when imperial requested,
                ForecastTime = DateTimeOffset.FromUnixTimeSeconds(openWeatherForecast.Date + openWeatherApiResponse.Location.TimezoneOffset).DateTime,
                CloudCover = openWeatherForecast.Clouds.CloudCover,
                WindSpeed = openWeatherForecast.Wind.WindSpeed,
                WindDirectionDegrees = openWeatherForecast.Wind.WindDirectionDegrees,
                WindDirection = CompassDirection.GetDirection(openWeatherForecast.Wind.WindDirectionDegrees),
                ExpectedRainfall = (openWeatherForecast.Rain?.RainfallThreeHours ?? 0.0) * 0.03937008,
                ExpectedSnowfall = (openWeatherForecast.Snow?.SnowfallThreeHours ?? 0.0) * 0.03937008
            };
            locationForecast.Forecasts.Add(forecast);
        }

        return locationForecast;
    }

    public async Task<CurrentWeather> GetCurrentWeatherAsync(string location)
    {
        var request = GetRequest($"/data/2.5/weather?q={location}&units=imperial&appid={_apiKey}", HttpMethod.Get);
        var response = await ExecuteAsync<CurrentConditionsResponse>(request, new CancellationToken());
        if (response == null)
        {
            return new CurrentWeather()
            {
                ErrorMessage = Status,
                Success = false
            };
        }
        return MapCurrentConditionsResponse(response);
    }

    public async Task<LocationForecast> GetForecastAsync(string location)
    {
        var request = GetRequest($"/data/2.5/forecast?q={location}&units=imperial&appid={_apiKey}", HttpMethod.Get);
        var response = await ExecuteAsync<ForecastResponse>(request, new CancellationToken());
        return MapForecastResponse(response);
    }


}
