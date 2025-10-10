using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Tsw6RealtimeWeather.Apis.Tsw6.Models;
using Tsw6RealtimeWeather.Configuration;

namespace Tsw6RealtimeWeather.Apis.Tsw6;

public class Tsw6ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline _retryPipeline;
    private ushort? _subscriptionId;

    public Tsw6ApiClient(string apiKey, RetryConfig? retryConfig = null)
    {
        retryConfig ??= new RetryConfig(); // Use defaults if not provided
        
        // Configure handler with shorter connection lifetimes to avoid connection aborts
        var handler = new SocketsHttpHandler
        {
            UseCookies = false,
            PooledConnectionLifetime = TimeSpan.FromSeconds(30),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(10),
            ConnectTimeout = TimeSpan.FromSeconds(5),
            MaxConnectionsPerServer = 1,
            EnableMultipleHttp2Connections = false
        };
        
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:31270"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Add the API key header to all requests automatically
        _httpClient.DefaultRequestHeaders.Add("DTGCommKey", apiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.ConnectionClose = true;
        
        // Configure retry policy with exponential backoff
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = retryConfig.MaxRetries,
                Delay = TimeSpan.FromMilliseconds(retryConfig.InitialDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    Logger.LogWarning($"HTTP request failed (attempt {args.AttemptNumber + 1}/{retryConfig.MaxRetries + 1}): {args.Outcome.Exception?.Message ?? "Unknown error"}");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
    
    /// <summary>
    /// Executes an HTTP operation with retry logic
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        return await _retryPipeline.ExecuteAsync(async ct => await operation());
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
            
            await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.PostAsync(requestUri, null);
                response.EnsureSuccessStatusCode();
                return true;
            });
            
            Logger.LogInfo($"Subscription registered with ID: {_subscriptionId}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"Failed to register subscription after retries: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Subscription registration timed out: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error during subscription registration: {ex.Message}");
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
            
            await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.DeleteAsync(requestUri);
                response.EnsureSuccessStatusCode();
                return true;
            });
            
            Logger.LogInfo($"Subscription {_subscriptionId} deregistered successfully");
            _subscriptionId = null; // Clear the subscription ID after successful deregistration
            return true;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"Failed to deregister subscription after retries: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Subscription deregistration timed out: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error during subscription deregistration: {ex.Message}");
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
            
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                
                var subscriptionData = await response.Content.ReadFromJsonAsync(Tsw6JsonContext.Default.Tsw6SubscriptionData);
                return subscriptionData;
            });
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"Failed to read subscription data after retries: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Subscription data read timed out: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error reading subscription data: {ex.Message}");
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
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.GetAsync("/info");
                response.EnsureSuccessStatusCode();
                
                var apiInfo = await response.Content.ReadFromJsonAsync(Tsw6JsonContext.Default.Tsw6ApiInfo);
                return apiInfo;
            });
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to get API info after retries: {ex.Message}");
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
