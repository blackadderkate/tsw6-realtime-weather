using Serilog;

namespace Tsw6RealtimeWeather;

public static class Logger
{
    static Logger()
    {
        // Configure Serilog to write to both console and file
        var logFilePath = Path.Combine(AppContext.BaseDirectory, "TSW6RealtimeWeather.log");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logFilePath,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        Log.Information("Logger initialized");
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
