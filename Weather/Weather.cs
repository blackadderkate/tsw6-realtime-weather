using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;

namespace Tsw6RealtimeWeather.Weather;

internal class RealtimeWeatherController
{
    private readonly Tsw6ApiClient tsw6ApiClient;
    private readonly OpenWeatherApiClient openWeatherApiClient;

    public RealtimeWeatherController(Tsw6ApiClient tsw6ApiClient, OpenWeatherApiClient openWeatherApiClient)
    {
        this.tsw6ApiClient = tsw6ApiClient;
        this.openWeatherApiClient = openWeatherApiClient;
    }

    internal async Task Initialise()
    {
        
    }

    internal async Task UpdateWeatherAsync()
    {


        Logger.LogInfo("Weather update");
    }
}