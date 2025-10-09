namespace Tsw6RealtimeWeather.Apis.OpenWeather;

public class OpenWeatherApiKey
{
    private static string apiKey = string.Empty;

    private static bool cachedApiKey = false;

    public static string Get()
    {
        if (cachedApiKey)
        {
            return apiKey;
        }

        apiKey = TryToGetApiKey();

        cachedApiKey = true;
        return apiKey;
    }

    private static string TryToGetApiKey()
    {
        var basePath = AppContext.BaseDirectory;
        var searchPath = Path.Combine(basePath, "WeatherApiKey.txt");
        if (!File.Exists(searchPath))
        {
            return string.Empty;
        }

        return File.ReadAllText(searchPath);
    }
}
