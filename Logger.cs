using System;
using System.IO;
using Serilog;
using Serilog.Events;


namespace Tsw6RealtimeWeather;

public static class Logger
{
    private static bool _initialized = false;

    /// <summary>
    /// Initialize the logger with a specific log level
    /// </summary>
    public static void Initialize(string logLevel = "Information")
    {
        if (_initialized)
        {
            return;
        }

        // Parse the log level string
        var minimumLevel = ParseLogLevel(logLevel);
        
        // Configure Serilog to write to file only (not console to avoid UI conflicts)
        var logFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                       "TSW6RealtimeWeather",
                                       "TSW6RealtimeWeather.log");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .WriteTo.File(
                logFilePath,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        _initialized = true;
        Log.Information($"Logger initialized with level: {minimumLevel}");
    }

    private static LogEventLevel ParseLogLevel(string level)
    {
        return level?.ToLowerInvariant() switch
        {
            "debug" => LogEventLevel.Debug,
            "information" or "info" => LogEventLevel.Information,
            "warning" or "warn" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    public static void LogInfo(string message)
    {
        Log.Information(message);
    }

    public static void LogWarning(string message)
    {
        Log.Warning(message);
    }

    public static void LogError(string message)
    {
        Log.Error(message);
    }

    public static void LogError(string message, Exception ex)
    {
        Log.Error(ex, message);
    }

    public static void LogDebug(string message)
    {
        Log.Debug(message);
    }

    public static void Close()
    {
        Log.CloseAndFlush();
    }
}
