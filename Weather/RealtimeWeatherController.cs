using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;

namespace Tsw6RealtimeWeather.Weather;

internal class RealtimeWeatherController
{
    private readonly Tsw6ApiClient _tsw6ApiClient;
    private readonly OpenWeatherApiClient _openWeatherApiClient;
    private PlayerLocation _playerLocation;

    public RealtimeWeatherController(Tsw6ApiClient tsw6ApiClient, OpenWeatherApiClient openWeatherApiClient)
    {
        _tsw6ApiClient = tsw6ApiClient;
        _openWeatherApiClient = openWeatherApiClient;
        _playerLocation = PlayerLocation.Default();
    }

    internal async Task InitialiseAsync()
    {
        await _tsw6ApiClient.RegisterSubscription();
    }

    internal async Task UpdatePlayerLocationAsync()
    {
        var newPlayerLocation = await ReadPlayerLocationAsync();
        
        if (newPlayerLocation == null)
        {
            Logger.LogWarning("Could not retrieve player location - skipping weather update");
            return;
        }

        // Calculate distance travelled since last update
        var distanceMeters = _playerLocation.DistanceToInMeters(newPlayerLocation);
        var distanceKm = _playerLocation.DistanceToInKilometers(newPlayerLocation);
        
        Logger.LogInfo($"Player location: {newPlayerLocation}");
        
        if (distanceMeters > 0.1) // Only log if moved more than 10cm
        {
            if (distanceKm >= 1.0)
            {
                Logger.LogInfo($"Distance travelled: {distanceKm:F2} km");
            }
            else
            {
                Logger.LogInfo($"Distance travelled: {distanceMeters:F1} m");
            }
        }
        
        _playerLocation = newPlayerLocation;
        
        // TODO: Call OpenWeather API with latitude and longitude
        // var weatherData = await _openWeatherApiClient.GetWeatherAsync(newPlayerLocation.Latitude, newPlayerLocation.Longitude);
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