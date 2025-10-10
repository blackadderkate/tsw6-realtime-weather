using YamlDotNet.Serialization;

namespace Tsw6RealtimeWeather.Configuration;

/// <summary>
/// Application configuration loaded from config.yaml
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Weather-related configuration
    /// </summary>
    [YamlMember(Alias = "weather")]
    public WeatherConfig Weather { get; set; } = new();

    /// <summary>
    /// Update interval configuration
    /// </summary>
    [YamlMember(Alias = "update")]
    public UpdateConfig Update { get; set; } = new();

    /// <summary>
    /// API keys configuration
    /// </summary>
    [YamlMember(Alias = "api_keys")]
    public ApiKeysConfig ApiKeys { get; set; } = new();
}

/// <summary>
/// Weather-specific configuration
/// </summary>
public class WeatherConfig
{
    /// <summary>
    /// Distance threshold in kilometers before updating weather (default: 10.0 km)
    /// </summary>
    [YamlMember(Alias = "update_threshold_km")]
    public double UpdateThresholdKm { get; set; } = 10.0;
}

/// <summary>
/// Update interval configuration
/// </summary>
public class UpdateConfig
{
    /// <summary>
    /// Delay in seconds between player location checks (default: 60 seconds)
    /// </summary>
    [YamlMember(Alias = "location_check_interval_seconds")]
    public int LocationCheckIntervalSeconds { get; set; } = 60;
}

/// <summary>
/// API keys configuration
/// </summary>
public class ApiKeysConfig
{
    /// <summary>
    /// OpenWeather API key (can be left empty to use WeatherApiKey.txt file)
    /// </summary>
    [YamlMember(Alias = "openweather")]
    public string? OpenWeather { get; set; }
}
