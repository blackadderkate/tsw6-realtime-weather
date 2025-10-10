using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;

namespace Tsw6RealtimeWeather.Weather;

internal class RealtimeWeatherController
{
    private const double WeatherUpdateThresholdKm = 10.0; // Update weather every 10km
    
    private readonly Tsw6ApiClient _tsw6ApiClient;
    private readonly OpenWeatherApiClient _openWeatherApiClient;
    private PlayerLocation _playerLocation;
    private PlayerLocation _lastWeatherUpdateLocation;
    private double _accumulatedDistanceKm;

    public RealtimeWeatherController(Tsw6ApiClient tsw6ApiClient, OpenWeatherApiClient openWeatherApiClient)
    {
        _tsw6ApiClient = tsw6ApiClient;
        _openWeatherApiClient = openWeatherApiClient;
        _playerLocation = PlayerLocation.Default();
        _lastWeatherUpdateLocation = PlayerLocation.Default();
        _accumulatedDistanceKm = 0.0;
    }

    internal async Task InitialiseAsync()
    {
        await _tsw6ApiClient.RegisterSubscription();
        
        // Get initial location and fetch weather
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

        // Calculate distance travelled since last update
        var distanceMeters = _playerLocation.DistanceToInMeters(newPlayerLocation);
        var distanceKm = _playerLocation.DistanceToInKilometers(newPlayerLocation);
        
        Logger.LogInfo($"Player location: {newPlayerLocation}");
        
        if (distanceMeters > 0.1) // Only log if moved more than 10cm
        {
            // Accumulate distance
            _accumulatedDistanceKm += distanceKm;
            
            if (distanceKm >= 1.0)
            {
                Logger.LogInfo($"Distance travelled: {distanceKm:F2} km (Total: {_accumulatedDistanceKm:F2} km)");
            }
            else
            {
                Logger.LogInfo($"Distance travelled: {distanceMeters:F1} m (Total: {_accumulatedDistanceKm:F2} km)");
            }
            
            // Check if we've crossed the weather update threshold
            if (_accumulatedDistanceKm >= WeatherUpdateThresholdKm)
            {
                Logger.LogInfo($"Accumulated distance ({_accumulatedDistanceKm:F2} km) exceeded threshold ({WeatherUpdateThresholdKm} km) - updating weather");
                await UpdateWeatherDataAsync(newPlayerLocation);
                
                // Reset accumulated distance and update last weather location
                _accumulatedDistanceKm = 0.0;
                _lastWeatherUpdateLocation = newPlayerLocation;
            }
        }
        
        _playerLocation = newPlayerLocation;
    }

    /// <summary>
    /// Fetches weather data for the given location
    /// </summary>
    private async Task UpdateWeatherDataAsync(PlayerLocation location)
    {
        Logger.LogInfo($"Fetching weather data for location: {location}");
        
        // TODO: Implement OpenWeather API call
        // var weatherData = await _openWeatherApiClient.GetWeatherAsync(location.Latitude, location.Longitude);
        // Logger.LogInfo($"Weather: {weatherData}");
        
        await Task.CompletedTask; // Remove this when implementing the actual API call
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