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
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:31270/"),
            Timeout = TimeSpan.FromSeconds(2)
        };
        
        // Add the API key header to all requests automatically
        _httpClient.DefaultRequestHeaders.Add("DTGCommKey", apiKey);
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
                _subscriptionId = (ushort)(Random.Shared.Next(1, ushort.MaxValue + 1));
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

    public async Task<string> GetAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"HTTP request failed: {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Request timed out: {ex.Message}");
            throw;
        }
    }

    public async Task<string> PostAsync(string endpoint, string jsonContent)
    {
        try
        {
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"HTTP request failed: {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Request timed out: {ex.Message}");
            throw;
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
            
            var apiInfo = await response.Content.ReadFromJsonAsync<Tsw6ApiInfo>();
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

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"API availability check failed: {ex.Message}");
            return false;
        }
    }
}
