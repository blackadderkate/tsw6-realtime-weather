using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;
using Tsw6RealtimeWeather.Weather;

namespace Tsw6RealtimeWeather
{

    class Tsw6RealtimeWeather
    {
        public static async Task Main(string[] args)
        {
            Logger.LogInfo("Searching for TSW6 API key...");
            var tsw6ApiKey = Tsw6ApiKey.Get();

            Logger.LogInfo("Searching for OpenWeather API key...");
            var weatherApiKey = OpenWeatherApiKey.Get();

            if (string.IsNullOrEmpty(tsw6ApiKey))
            {
                Logger.LogWarning("No TSW6 API Key found. Please launch TSW6 with the -HTTPAPI flag set.");
            }
            else
            {
                Logger.LogInfo($"Found TSW6 API key: {tsw6ApiKey}");
            }

            if (string.IsNullOrEmpty(weatherApiKey))
            {
                Logger.LogWarning("No OpenWeather API Key found. Please save a file named WeatherApiKey.txt in the same folder as this program.");
            }
            else
            {
                Logger.LogInfo($"Found OpenWeather API key: {weatherApiKey}");
            }

            Logger.LogInfo("Checking for TSW6 server...");
            var tsw6ApiClient = new Tsw6ApiClient(tsw6ApiKey);
            var isTsw6Active = await tsw6ApiClient.IsApiAvailableAsync();
            if (!isTsw6Active)
            {
                Logger.LogError("TSW6 HTTP Server isn't accessible, check TSW6 is running with the -HTTPAPI flag set.");
                Logger.Close();
                return;
            }

            var weather = new RealtimeWeatherController(new Tsw6ApiClient(tsw6ApiKey), new OpenWeatherApiClient());

            // Main update loop - runs every 1 minute
            Logger.LogInfo("Starting weather update loop (every 1 minute). Press Ctrl+C to exit.");
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
                Logger.LogInfo("Shutting down...");
            };

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    Logger.LogInfo("Updating weather...");
                    await weather.UpdateWeatherAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error during update: {ex.Message}", ex);
                }
            }
            
            Logger.Close();
        }

    }

}
