using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Configuration;

/// <summary>
/// JSON serialization context for AOT compatibility
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(AppConfig))]
internal partial class ConfigJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Handles loading and saving application configuration from JSON files
/// </summary>
public static class ConfigManager
{
    private const string ConfigFileName = "config.json";
    
    /// <summary>
    /// Loads configuration from config.json, creates default if it doesn't exist
    /// </summary>
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
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.AppConfig);
            
            if (config == null)
            {
                Logger.LogWarning("Configuration deserialized to null, using default configuration");
                return CreateDefaultConfig();
            }
            
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
    /// Saves configuration to config.json
    /// </summary>
    public static void SaveConfig(AppConfig config)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        
        try
        {
            var json = JsonSerializer.Serialize(config, ConfigJsonContext.Default.AppConfig);
            File.WriteAllText(configPath, json);
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
