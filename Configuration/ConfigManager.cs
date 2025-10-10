using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tsw6RealtimeWeather.Configuration;

/// <summary>
/// Handles loading and saving application configuration from YAML files
/// </summary>
public static class ConfigManager
{
    private const string ConfigFileName = "config.yaml";
    
    /// <summary>
    /// Loads configuration from config.yaml, creates default if it doesn't exist
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Configuration is loaded at startup before AOT compilation affects runtime behavior")]
    public static AppConfig LoadConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        
        if (!File.Exists(configPath))
        {
            Logger.LogInfo($"Configuration file not found at {configPath}, creating default configuration");
            var defaultConfig = CreateDefaultConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        try
        {
            var yaml = File.ReadAllText(configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            
            var config = deserializer.Deserialize<AppConfig>(yaml);
            Logger.LogInfo($"Configuration loaded from {configPath}");
            LogConfigValues(config);
            
            return config;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load configuration from {configPath}: {ex.Message}");
            Logger.LogWarning("Using default configuration");
            return CreateDefaultConfig();
        }
    }

    /// <summary>
    /// Saves configuration to config.yaml
    /// </summary>
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Configuration is saved at startup before AOT compilation affects runtime behavior")]
    public static void SaveConfig(AppConfig config)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            
            var yaml = serializer.Serialize(config);
            
            // Add comments to the YAML file
            var yamlWithComments = AddConfigComments(yaml);
            
            File.WriteAllText(configPath, yamlWithComments);
            Logger.LogInfo($"Configuration saved to {configPath}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to save configuration to {configPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a default configuration with reasonable defaults
    /// </summary>
    private static AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            Weather = new WeatherConfig
            {
                UpdateThresholdKm = 10.0
            },
            Update = new UpdateConfig
            {
                LocationCheckIntervalSeconds = 60
            },
            ApiKeys = new ApiKeysConfig
            {
                OpenWeather = ""
            }
        };
    }

    /// <summary>
    /// Adds helpful comments to the generated YAML
    /// </summary>
    private static string AddConfigComments(string yaml)
    {
        var lines = new List<string>
        {
            "# TSW6 Realtime Weather Configuration",
            "# This file controls various aspects of the weather synchronization",
            "",
            "# Weather update settings",
            yaml,
            "",
            "# Notes:",
            "# - weather.update_threshold_km: Distance in kilometers before fetching new weather data",
            "#   Recommended: 10-50 km depending on how often you want weather updates",
            "#",
            "# - update.location_check_interval_seconds: How often to check player location",
            "#   Recommended: 30-120 seconds for balance between accuracy and performance",
            "#",
            "# - retry.max_retries: Maximum number of retry attempts for failed HTTP requests",
            "#   Default: 5 retries with exponential backoff",
            "#",
            "# - retry.initial_delay_ms: Initial delay before first retry in milliseconds",
            "#   Subsequent retries use exponential backoff (e.g., 100ms, 200ms, 400ms, 800ms, 1600ms)",
            "#",
            "# - logging.level: Minimum logging level (Debug, Information, Warning, Error)",
            "#   Debug: Verbose logging for troubleshooting",
            "#   Information: Standard operational logging (recommended)",
            "#   Warning: Only warnings and errors",
            "#   Error: Only errors",
            "#",
            "# - api_keys.openweather: Your OpenWeather API key",
            "#   Leave empty to use WeatherApiKey.txt file instead",
            "#   Get a free key at: https://openweathermap.org/api"
        };
        
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Logs the loaded configuration values
    /// </summary>
    private static void LogConfigValues(AppConfig config)
    {
        Logger.LogInfo($"  Weather update threshold: {config.Weather.UpdateThresholdKm} km");
        Logger.LogInfo($"  Location check interval: {config.Update.LocationCheckIntervalSeconds} seconds");
        Logger.LogInfo($"  HTTP max retries: {config.Retry.MaxRetries}");
        Logger.LogInfo($"  HTTP initial delay: {config.Retry.InitialDelayMs} ms");
        Logger.LogInfo($"  Logging level: {config.Logging.Level}");
        
        if (!string.IsNullOrEmpty(config.ApiKeys.OpenWeather))
        {
            Logger.LogInfo($"  OpenWeather API key: {MaskApiKey(config.ApiKeys.OpenWeather)}");
        }
    }

    /// <summary>
    /// Masks an API key for logging (shows first 4 and last 4 characters)
    /// </summary>
    private static string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
        {
            return "****";
        }
        
        return $"{apiKey[..4]}...{apiKey[^4..]}";
    }
}
