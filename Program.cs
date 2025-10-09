using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;
using Tsw6RealtimeWeather.Weather;

namespace Tsw6RealtimeWeather
{

    class Tsw6RealtimeWeather
    {
        private static Tsw6ApiClient? _tsw6ApiClient;

        public static async Task Main(string[] args)
        {
            // Set up cleanup handlers for graceful shutdown
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Console.CancelKeyPress += OnCancelKeyPress;

            try
            {
                await RunApplicationAsync();
            }
            finally
            {
                await CleanupAsync();
            }
        }

        private static async Task RunApplicationAsync()
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
            _tsw6ApiClient = new Tsw6ApiClient(tsw6ApiKey);
            var isTsw6Active = await _tsw6ApiClient.IsApiAvailableAsync();
            if (!isTsw6Active)
            {
                Logger.LogError("TSW6 HTTP Server isn't accessible, check TSW6 is running with the -HTTPAPI flag set.");
                return;
            }

            var weather = new RealtimeWeatherController(_tsw6ApiClient, new OpenWeatherApiClient());
            await weather.Initialise();

            // Main update loop - runs every 1 minute
            Logger.LogInfo("Starting weather update loop (every 1 minute). Press Ctrl+C to exit.");
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
                Logger.LogInfo("Shutdown requested...");
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
        }

        private static async Task CleanupAsync()
        {
            Logger.LogInfo("Performing cleanup...");
            
            if (_tsw6ApiClient?.GetSubscriptionId() != null)
            {
                Logger.LogInfo("Deregistering subscription...");
                await _tsw6ApiClient.DeregisterSubscription();
            }
            
            Logger.Close();
        }

        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            // Already handled in the main loop
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            Logger.LogInfo("Process exit detected");
            CleanupAsync().GetAwaiter().GetResult();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogError($"Unhandled exception: {e.ExceptionObject}");
            CleanupAsync().GetAwaiter().GetResult();
        }

    }

}
