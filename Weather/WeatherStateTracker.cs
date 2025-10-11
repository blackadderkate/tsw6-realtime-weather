using System;
using System.Threading;
using System.Threading.Tasks;
using Tsw6RealtimeWeather.Apis.Tsw6;
using Tsw6RealtimeWeather.Apis.Tsw6.Models;
using Tsw6RealtimeWeather.UI;

namespace Tsw6RealtimeWeather.Weather;

/// <summary>
/// Tracks weather state and handles smooth transitions between weather updates
/// </summary>
public class WeatherStateTracker
{
    private readonly Tsw6ApiClient _tsw6ApiClient;
    private readonly ConsoleUI? _ui;
    private readonly int _transitionDurationSeconds;
    
    private Tsw6WeatherData _currentState;
    private Tsw6WeatherData? _targetState;
    private DateTime _transitionStartTime;
    private CancellationTokenSource? _transitionCancellation;
    private Task? _transitionTask;

    public WeatherStateTracker(Tsw6ApiClient tsw6ApiClient, int transitionDurationSeconds = 30, ConsoleUI? ui = null)
    {
        _tsw6ApiClient = tsw6ApiClient;
        _ui = ui;
        _transitionDurationSeconds = transitionDurationSeconds;
        _currentState = new Tsw6WeatherData();
    }

    /// <summary>
    /// Updates the target weather state and begins smooth transition
    /// </summary>
    public async Task TransitionToWeatherAsync(Tsw6WeatherData newWeather)
    {
        // Cancel any existing transition
        if (_transitionCancellation != null)
        {
            _transitionCancellation.Cancel();
            if (_transitionTask != null)
            {
                try
                {
                    await _transitionTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                }
            }
        }

        _targetState = newWeather;
        _transitionStartTime = DateTime.UtcNow;
        _transitionCancellation = new CancellationTokenSource();

        Logger.LogInfo($"Starting weather transition over {_transitionDurationSeconds} seconds");
        Logger.LogDebug($"From: Temp={_currentState.Temperature:F1}°C, Cloud={_currentState.Cloudiness:F2}, Precip={_currentState.Precipitation:F2}");
        Logger.LogDebug($"To: Temp={newWeather.Temperature:F1}°C, Cloud={newWeather.Cloudiness:F2}, Precip={newWeather.Precipitation:F2}");

        _transitionTask = RunTransitionAsync(_transitionCancellation.Token);
    }

    /// <summary>
    /// Runs the smooth transition loop, updating weather every second
    /// </summary>
    private async Task RunTransitionAsync(CancellationToken cancellationToken)
    {
        if (_targetState == null)
            return;

        var startState = CloneWeatherData(_currentState);
        var endState = _targetState;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsed = (DateTime.UtcNow - _transitionStartTime).TotalSeconds;
                var progress = Math.Min(elapsed / _transitionDurationSeconds, 1.0);

                // Linear interpolation
                _currentState = InterpolateWeather(startState, endState, progress);

                // Send update to TSW6
                await _tsw6ApiClient.UpdateWeatherAsync(_currentState);

                // Update UI with current game weather state
                _ui?.UpdateGameWeather(_currentState);

                if (progress >= 1.0)
                {
                    Logger.LogInfo("Weather transition complete");
                    Logger.LogDebug($"Final state: Temp={_currentState.Temperature:F1}°C, Cloud={_currentState.Cloudiness:F2}, Precip={_currentState.Precipitation:F2}");
                    break;
                }

                // Wait 1 second before next update
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Weather transition cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error during weather transition: {ex.Message}");
        }
    }

    /// <summary>
    /// Linearly interpolates between two weather states
    /// </summary>
    private Tsw6WeatherData InterpolateWeather(Tsw6WeatherData start, Tsw6WeatherData end, double progress)
    {
        return new Tsw6WeatherData
        {
            Temperature = Lerp(start.Temperature, end.Temperature, progress),
            Cloudiness = Lerp(start.Cloudiness, end.Cloudiness, progress),
            Precipitation = Lerp(start.Precipitation, end.Precipitation, progress),
            Wetness = Lerp(start.Wetness, end.Wetness, progress),
            GroundSnow = Lerp(start.GroundSnow, end.GroundSnow, progress),
            FogDensity = Lerp(start.FogDensity, end.FogDensity, progress)
        };
    }

    /// <summary>
    /// Linear interpolation between two values
    /// </summary>
    private double Lerp(double start, double end, double progress)
    {
        return start + (end - start) * progress;
    }

    /// <summary>
    /// Creates a deep copy of weather data
    /// </summary>
    private static Tsw6WeatherData CloneWeatherData(Tsw6WeatherData source)
    {
        return new Tsw6WeatherData
        {
            Temperature = source.Temperature,
            Cloudiness = source.Cloudiness,
            Precipitation = source.Precipitation,
            Wetness = source.Wetness,
            GroundSnow = source.GroundSnow,
            FogDensity = source.FogDensity
        };
    }

    /// <summary>
    /// Gets the current weather state
    /// </summary>
    public Tsw6WeatherData GetCurrentState() => CloneWeatherData(_currentState);

    /// <summary>
    /// Stops any active transition
    /// </summary>
    public async Task StopTransitionAsync()
    {
        if (_transitionCancellation != null)
        {
            _transitionCancellation.Cancel();
            if (_transitionTask != null)
            {
                try
                {
                    await _transitionTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected
                }
            }
        }
    }
}
