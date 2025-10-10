using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Tsw6RealtimeWeather.Apis.OpenWeather.Models;
using Tsw6RealtimeWeather.Configuration;

namespace Tsw6RealtimeWeather.Apis.OpenWeather;

public class OpenWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ResiliencePipeline _retryPipeline;
    private const string BaseUrl = "https://api.openweathermap.org";

    public OpenWeatherApiClient(string apiKey, RetryConfig? retryConfig = null)
    {
        _apiKey = apiKey;

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromSeconds(30),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(10),
            ConnectTimeout = TimeSpan.FromSeconds(5),
            MaxConnectionsPerServer = 1
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };

        _httpClient.DefaultRequestHeaders.ConnectionClose = true;

        var maxRetries = retryConfig?.MaxRetries ?? 5;
        var initialDelay = retryConfig?.InitialDelayMs ?? 100;

        var retryOptions = new RetryStrategyOptions
        {
            MaxRetryAttempts = maxRetries,
            Delay = TimeSpan.FromMilliseconds(initialDelay),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                Logger.LogWarning(
                    $"HTTP request failed (attempt {args.AttemptNumber + 1}/{maxRetries + 1}): {args.Outcome.Exception?.Message}");
                return ValueTask.CompletedTask;
            }
        };

        _retryPipeline = new ResiliencePipelineBuilder().AddRetry(retryOptions).Build();
    }

    /// <summary>
    /// Generic wrapper for HTTP operations with retry logic
    /// </summary>
    private async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        return await _retryPipeline.ExecuteAsync(async token => await operation());
    }

    /// <summary>
    /// Fetches current weather data for the specified coordinates
    /// </summary>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <returns>OpenWeatherResponse with current weather data, or null if request fails</returns>
    public async Task<OpenWeatherResponse?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        try
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var url = $"/data/2.5/weather?lat={latitude}&lon={longitude}&appid={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync(
                    OpenWeatherJsonContext.Default.OpenWeatherResponse);
            });
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError($"Failed to fetch weather data after retries: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError($"Weather request timed out after retries: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error fetching weather data after retries: {ex.Message}");
            return null;
        }
    }
}
