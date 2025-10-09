using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;

namespace Tsw6RealtimeWeather.Weather;

internal class RealtimeWeatherController
{
    private readonly Tsw6ApiClient tsw6ApiClient;
    private readonly OpenWeatherApiClient openWeatherApiClient;

    public RealtimeWeatherController(Tsw6ApiClient tsw6ApiClient, OpenWeatherApiClient openWeatherApiClient)
    {
        this.tsw6ApiClient = tsw6ApiClient;
        this.openWeatherApiClient = openWeatherApiClient;
    }

    internal async Task Initialise()
    {
        await tsw6ApiClient.RegisterSubscription();
    }

    internal async Task UpdateWeatherAsync()
    {
        var location = await ReadPlayerLocationAsync();
        
        if (!location.HasValue)
        {
            Logger.LogWarning("Could not retrieve player location - skipping weather update");
            return;
        }

        var (latitude, longitude) = location.Value;
        Logger.LogInfo($"Player location: Lat={latitude:F6}, Lon={longitude:F6}");
        
        // TODO: Call OpenWeather API with latitude and longitude
        // var weatherData = await openWeatherApiClient.GetWeatherAsync(latitude, longitude);
    }

    /// <summary>
    /// Reads the player's current geographic location and returns it as a simple (latitude, longitude) pair
    /// </summary>
    /// <returns>A tuple containing (latitude, longitude) as doubles, or null if the location cannot be determined</returns>
    internal async Task<(double Latitude, double Longitude)?> ReadPlayerLocationAsync()
    {
        try
        {
            var subscriptionData = await tsw6ApiClient.ReadPlayerInformationAsync();
            
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
            
            return (latitude, longitude);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error reading player location: {ex.Message}");
            return null;
        }
    }
}