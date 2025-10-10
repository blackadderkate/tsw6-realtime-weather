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
    /// HTTP retry configuration
    /// </summary>
    [YamlMember(Alias = "retry")]
    public RetryConfig Retry { get; set; } = new();

    /// <summary>
    /// Logging configuration
    /// </summary>
    [YamlMember(Alias = "logging")]
    public LoggingConfig Logging { get; set; } = new();

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
/// HTTP retry configuration
/// </summary>
public class RetryConfig
{
    /// <summary>
    /// Maximum number of retry attempts for failed HTTP requests (default: 5)
    /// </summary>
    [YamlMember(Alias = "max_retries")]
    public int MaxRetries { get; set; } = 5;
    
    /// <summary>
    /// Initial delay in milliseconds before first retry (default: 100ms)
    /// Subsequent retries use exponential backoff
    /// </summary>
    [YamlMember(Alias = "initial_delay_ms")]
    public int InitialDelayMs { get; set; } = 100;
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingConfig
{
    /// <summary>
    /// Minimum logging level: Debug, Information, Warning, Error (default: Information)
    /// </summary>
    [YamlMember(Alias = "level")]
    public string Level { get; set; } = "Information";
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
