using System;
using System.Threading.Tasks;
using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;
using Tsw6RealtimeWeather.Configuration;
using Tsw6RealtimeWeather.UI;

namespace Tsw6RealtimeWeather.Weather;

internal class RealtimeWeatherController
{
    private readonly double _weatherUpdateThresholdKm;
    private readonly Tsw6ApiClient _tsw6ApiClient;
    private readonly OpenWeatherApiClient? _openWeatherApiClient;
    private readonly ConsoleUI _ui;
    private readonly bool _hasWeatherApiKey;
    private PlayerLocation _playerLocation;
    private PlayerLocation _lastWeatherUpdateLocation;
    private double _accumulatedDistanceKm;

    public RealtimeWeatherController(Tsw6ApiClient tsw6ApiClient, OpenWeatherApiClient? openWeatherApiClient, AppConfig config, ConsoleUI ui)
    {
        _tsw6ApiClient = tsw6ApiClient;
        _openWeatherApiClient = openWeatherApiClient;
        _ui = ui;
        _weatherUpdateThresholdKm = config.Weather.UpdateThresholdKm;
        _playerLocation = PlayerLocation.Default();
        _lastWeatherUpdateLocation = PlayerLocation.Default();
        _accumulatedDistanceKm = 0.0;
        _hasWeatherApiKey = openWeatherApiClient != null;
        
        if (!_hasWeatherApiKey)
        {
            Logger.LogWarning("Weather controller initialized without OpenWeather API key - weather sync disabled");
        }
        else
        {
            Logger.LogInfo($"Weather controller initialized with {_weatherUpdateThresholdKm} km update threshold");
        }
    }

    internal async Task InitialiseAsync()
    {
        await _tsw6ApiClient.RegisterSubscription();
        
        var initialLocation = await ReadPlayerLocationAsync();
        if (initialLocation != null)
        {
            _playerLocation = initialLocation;
            _lastWeatherUpdateLocation = initialLocation;
            await UpdateWeatherDataAsync(initialLocation);
        }
    }

    internal async Task UpdatePlayerLocationAsync()
    {
        var newPlayerLocation = await ReadPlayerLocationAsync();
        
        if (newPlayerLocation == null)
        {
            Logger.LogWarning("Could not retrieve player location - skipping update");
            return;
        }

        var distanceMeters = _playerLocation.DistanceToInMeters(newPlayerLocation);
        var distanceKm = _playerLocation.DistanceToInKilometers(newPlayerLocation);
        
        Logger.LogDebug($"Player location: {newPlayerLocation}");
        
        if (distanceMeters > 0.1)
        {
            _accumulatedDistanceKm += distanceKm;
            
            Logger.LogDebug($"Distance travelled: {distanceKm:F2} km (Total: {_accumulatedDistanceKm:F2} km)");
            
            if (_accumulatedDistanceKm >= _weatherUpdateThresholdKm)
            {
                Logger.LogInfo($"Accumulated distance ({_accumulatedDistanceKm:F2} km) exceeded threshold ({_weatherUpdateThresholdKm} km) - updating weather");
                await UpdateWeatherDataAsync(newPlayerLocation);
                
                _accumulatedDistanceKm = 0.0;
                _lastWeatherUpdateLocation = newPlayerLocation;
            }
        }
        
        _ui.UpdateDistance(_accumulatedDistanceKm, newPlayerLocation.ToString());
        
        _playerLocation = newPlayerLocation;
    }

    private async Task UpdateWeatherDataAsync(PlayerLocation location)
    {
        if (!_hasWeatherApiKey || _openWeatherApiClient == null)
        {
            Logger.LogDebug("Skipping weather update - no OpenWeather API key configured");
            _ui.UpdateWeather("⚠ No OpenWeather API key - configure in config.json or WeatherApiKey.txt");
            return;
        }
        
        Logger.LogInfo($"Fetching weather data for location: {location}");
        
        try
        {
            var weatherData = await _openWeatherApiClient.GetCurrentWeatherAsync(location.Latitude, location.Longitude);
            
            if (weatherData == null)
            {
                Logger.LogWarning("No weather data returned from API");
                _ui.UpdateWeather("⚠ Failed to fetch weather data");
                return;
            }

            _ui.UpdateWeather(weatherData);
            
            var tsw6Weather = WeatherConverter.ConvertToTsw6Weather(weatherData);
            
            var updateSuccess = await _tsw6ApiClient.UpdateWeatherAsync(tsw6Weather);
            
            if (updateSuccess)
            {
                Logger.LogInfo($"Weather synchronized to TSW6: {weatherData.Weather?[0]?.Main} - {weatherData.Main?.Temp}K");
            }
            else
            {
                Logger.LogWarning("Failed to update weather in TSW6, but UI was updated");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error updating weather data: {ex.Message}");
            _ui.UpdateWeather($"⚠ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the accumulated distance travelled since the last weather update
    /// </summary>
    public double GetAccumulatedDistanceKm() => _accumulatedDistanceKm;

    /// <summary>
    /// Forces a weather update regardless of distance threshold
    /// </summary>
    public async Task ForceWeatherUpdateAsync()
    {
        Logger.LogInfo("Forcing weather update");
        if (_playerLocation != null)
        {
            await UpdateWeatherDataAsync(_playerLocation);
            _accumulatedDistanceKm = 0.0;
            _lastWeatherUpdateLocation = _playerLocation;
        }
    }

    /// <summary>
    /// Reads the player's current geographic location
    /// </summary>
    /// <returns>A PlayerLocation object, or null if the location cannot be determined</returns>
    internal async Task<PlayerLocation?> ReadPlayerLocationAsync()
    {
        try
        {
            var subscriptionData = await _tsw6ApiClient.ReadPlayerInformationAsync();
            
            if (subscriptionData == null)
            {
                Logger.LogWarning("Failed to read player information - no subscription data returned");
                return null;
            }

            if (subscriptionData.Entries == null || subscriptionData.Entries.Count == 0)
            {
                Logger.LogWarning("No subscription entries found");
                return null;
            }

            var entry = subscriptionData.Entries[0];
            
            if (entry.Values == null)
            {
                Logger.LogWarning("Subscription entry has no values");
                return null;
            }

            if (entry.Values.GeoLocation == null)
            {
                Logger.LogWarning("No geo-location data available");
                return null;
            }

            var latitude = entry.Values.GeoLocation.Latitude;
            var longitude = entry.Values.GeoLocation.Longitude;

            Logger.LogDebug($"Player location: Latitude={latitude}, Longitude={longitude}");
            
            return new PlayerLocation(latitude, longitude);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error reading player location: {ex.Message}");
            return null;
        }
    }
}