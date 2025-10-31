using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Configuration;

/// <summary>
/// Application configuration loaded from config.json
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Weather-related configuration
    /// </summary>
    [JsonPropertyName("weather")]
    public WeatherConfig Weather { get; set; } = new();

    /// <summary>
    /// Update interval configuration
    /// </summary>
    [JsonPropertyName("update")]
    public UpdateConfig Update { get; set; } = new();

    /// <summary>
    /// HTTP retry configuration
    /// </summary>
    [JsonPropertyName("retry")]
    public RetryConfig Retry { get; set; } = new();

    /// <summary>
    /// Logging configuration
    /// </summary>
    [JsonPropertyName("logging")]
    public LoggingConfig Logging { get; set; } = new();

    /// <summary>
    /// API keys configuration
    /// </summary>
    [JsonPropertyName("api_keys")]
    public ApiKeysConfig ApiKeys { get; set; } = new();

    /// <summary>
    /// Quit if failed to contact TSW6 after this many update attempts
    /// </summary>
    [JsonPropertyName("closeafter")]
    public FailedUpdateAttemptCountConfig FailedUpdateAttemptCount { get; set; } = new();
}

/// <summary>
/// Weather-specific configuration
/// </summary>
public class WeatherConfig
{
    /// <summary>
    /// Distance threshold in kilometers before updating weather (default: 5.0 km)
    /// </summary>
    [JsonPropertyName("update_threshold_km")]
    public double UpdateThresholdKm { get; set; } = 5.0;

    /// <summary>
    /// Duration in seconds for smooth weather transitions (default: 30 seconds)
    /// </summary>
    [JsonPropertyName("transition_duration_seconds")]
    public int TransitionDurationSeconds { get; set; } = 30;
}

/// <summary>
/// Update interval configuration
/// </summary>
public class UpdateConfig
{
    /// <summary>
    /// Delay in seconds between player location checks (default: 5 seconds)
    /// </summary>
    [JsonPropertyName("location_check_interval_seconds")]
    public int LocationCheckIntervalSeconds { get; set; } = 5;
}

/// <summary>
/// HTTP retry configuration
/// </summary>
public class RetryConfig
{
    /// <summary>
    /// Maximum number of retry attempts for failed HTTP requests (default: 5)
    /// </summary>
    [JsonPropertyName("max_retries")]
    public int MaxRetries { get; set; } = 5;
    
    /// <summary>
    /// Initial delay in milliseconds before first retry (default: 100ms)
    /// Subsequent retries use exponential backoff
    /// </summary>
    [JsonPropertyName("initial_delay_ms")]
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
    [JsonPropertyName("level")]
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
    [JsonPropertyName("openweather")]
    public string? OpenWeather { get; set; }
}

/// <summary>
/// Quit if failed to contact TSW6 after this many update attempts
/// </summary>
public class FailedUpdateAttemptCountConfig
{
    /// <summary>
    /// Quit if failed to contact TSW6 after this many update attempts
    /// </summary>
    [JsonPropertyName("failed_update_attempts")]
    public int AttemptCount { get; set; } = 4;
}
