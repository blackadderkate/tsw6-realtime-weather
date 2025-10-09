using System.Net.Http.Json;
using Tsw6RealtimeWeather.Apis.Tsw6.Models;

namespace Tsw6RealtimeWeather.Apis.Tsw6;

public class Tsw6ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private ushort? _subscriptionId;

    public Tsw6ApiClient(string apiKey)
    {
        _apiKey = apiKey;
        
        // Configure handler to disable Nagle's algorithm for faster response times
        var handler = new SocketsHttpHandler
        {
            // Disable Nagle's algorithm - send data immediately without buffering
            UseCookies = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(1),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            ConnectTimeout = TimeSpan.FromSeconds(5)
        };
        
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:31270"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Add the API key header to all requests automatically
        _httpClient.DefaultRequestHeaders.Add("DTGCommKey", apiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.ConnectionClose = false; // Keep connection alive
    }

    /// <summary>
    /// Registers a subscription to DriverAid.PlayerInfo endpoint
    /// </summary>
    /// <returns>True if subscription was successful, false otherwise</returns>
    public async Task<bool> RegisterSubscription()
    {
        try
        {
            // Generate a random subscription ID if we don't have one
            if (!_subscriptionId.HasValue)
            {
                _subscriptionId = (ushort)Random.Shared.Next(1, ushort.MaxValue + 1);
            }

            var requestUri = $"/subscription/DriverAid.PlayerInfo?Subscription={_subscriptionId}";
            var response = await _httpClient.PostAsync(requestUri, null);
            response.EnsureSuccessStatusCode();
            
            Logger.LogInfo($"Subscription registered with ID: {_subscriptionId}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"Failed to register subscription: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Subscription registration timed out: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deregisters the current subscription
    /// </summary>
    /// <returns>True if deregistration was successful, false otherwise</returns>
    public async Task<bool> DeregisterSubscription()
    {
        if (!_subscriptionId.HasValue)
        {
            Logger.LogWarning("No subscription to deregister");
            return false;
        }

        try
        {
            var requestUri = $"/subscription/?Subscription={_subscriptionId}";
            var response = await _httpClient.DeleteAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            Logger.LogInfo($"Subscription {_subscriptionId} deregistered successfully");
            _subscriptionId = null; // Clear the subscription ID after successful deregistration
            return true;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"Failed to deregister subscription: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Subscription deregistration timed out: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the current subscription ID
    /// </summary>
    /// <returns>The subscription ID, or null if not yet registered</returns>
    public ushort? GetSubscriptionId() => _subscriptionId;

    /// <summary>
    /// Reads data from the current subscription
    /// </summary>
    /// <returns>The subscription data, or null if the request fails or no subscription is active</returns>
    public async Task<Tsw6SubscriptionData?> ReadPlayerInformationAsync()
    {
        if (!_subscriptionId.HasValue)
        {
            Logger.LogWarning("No active subscription to read from");
            return null;
        }

        try
        {
            var requestUri = $"/subscription?Subscription={_subscriptionId}";
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            var subscriptionData = await response.Content.ReadFromJsonAsync(Tsw6JsonContext.Default.Tsw6SubscriptionData);
            return subscriptionData;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"Failed to read subscription data: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Subscription data read timed out: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets API information including metadata and available routes
    /// </summary>
    /// <returns>The API info object, or null if the request fails</returns>
    public async Task<Tsw6ApiInfo?> GetApiInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/info");
            response.EnsureSuccessStatusCode();
            
            var apiInfo = await response.Content.ReadFromJsonAsync(Tsw6JsonContext.Default.Tsw6ApiInfo);
            return apiInfo;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get API info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if the TSW6 API is reachable and returns valid metadata
    /// </summary>
    /// <returns>True if the API is reachable and returns valid TSW6 metadata, false otherwise</returns>
    public async Task<bool> IsApiAvailableAsync()
    {
        try
        {
            var apiInfo = await GetApiInfoAsync();
            
            if (apiInfo == null)
            {
                Logger.LogError("Failed to retrieve API info");
                return false;
            }

            // Validate Meta object exists
            if (apiInfo.Meta == null)
            {
                Logger.LogError("API response missing 'Meta' property");
                return false;
            }

            // Validate required fields
            if (apiInfo.Meta.Worker != "DTGCommWorkerRC")
            {
                Logger.LogError($"Invalid Worker value: {apiInfo.Meta.Worker}");
                return false;
            }

            if (apiInfo.Meta.GameName != "Train Sim World 6®")
            {
                Logger.LogError($"Invalid GameName value: {apiInfo.Meta.GameName}");
                return false;
            }

            if (apiInfo.Meta.GameBuildNumber <= 0)
            {
                Logger.LogError("Invalid or missing GameBuildNumber");
                return false;
            }

            if (apiInfo.Meta.APIVersion <= 0)
            {
                Logger.LogError("Invalid or missing APIVersion");
                return false;
            }

            if (string.IsNullOrEmpty(apiInfo.Meta.GameInstanceID))
            {
                Logger.LogError("Missing GameInstanceID");
                return false;
            }

            Logger.LogInfo($"Connected to API with GameInstanceID {apiInfo.Meta.GameInstanceID} on version {apiInfo.Meta.GameBuildNumber}");

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"API availability check failed: {ex.Message}");
            return false;
        }
    }
}
