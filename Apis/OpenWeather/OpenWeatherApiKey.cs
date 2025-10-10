using System;
using System.IO;

namespace Tsw6RealtimeWeather.Apis.OpenWeather;

public class OpenWeatherApiKey
{
    private static string apiKey = string.Empty;
    private static bool cachedApiKey = false;

    /// <summary>
    /// Gets the OpenWeather API key from config, or falls back to WeatherApiKey.txt file
    /// </summary>
    public static string Get(string? configApiKey = null)
    {
        if (!string.IsNullOrWhiteSpace(configApiKey))
        {
            Logger.LogInfo("Using OpenWeather API key from config.json");
            return configApiKey;
        }

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
