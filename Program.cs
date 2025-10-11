using System;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Tsw6RealtimeWeather.Apis.OpenWeather;
using Tsw6RealtimeWeather.Apis.Tsw6;
using Tsw6RealtimeWeather.Configuration;
using Tsw6RealtimeWeather.UI;
using Tsw6RealtimeWeather.Weather;

namespace Tsw6RealtimeWeather
{
    internal class Program
    {
        private static Tsw6ApiClient? _tsw6ApiClient;
        private static ConsoleUI? _ui;

        public static async Task Main(string[] args)
        {
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
            var config = ConfigManager.LoadConfig();
            
            Logger.Initialize(config.Logging.Level);

            // Show title before starting interactive displays
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("TSW6 Weather")
                    .LeftJustified()
                    .Color(Color.Cyan1));
            AnsiConsole.MarkupLine("[dim]Real-time weather sync for Train Sim World 6[/]\n");

            Logger.LogInfo("Searching for TSW6 API key...");
            var tsw6ApiKey = Tsw6ApiKey.Get();

            Logger.LogInfo("Searching for OpenWeather API key...");
            var weatherApiKey = OpenWeatherApiKey.Get(config.ApiKeys.OpenWeather);

            bool apiKeysFound = !string.IsNullOrEmpty(tsw6ApiKey) && !string.IsNullOrEmpty(weatherApiKey);
            
            if (string.IsNullOrEmpty(tsw6ApiKey))
            {
                Logger.LogWarning("No TSW6 API Key found. Please launch TSW6 with the -HTTPAPI flag set.");
            }

            if (string.IsNullOrEmpty(weatherApiKey))
            {
                Logger.LogWarning("No OpenWeather API Key found. Please save a file named WeatherApiKey.txt in the same folder as this program.");
            }

            _tsw6ApiClient = new Tsw6ApiClient(tsw6ApiKey, config.Retry);
            
            bool tsw6Connected = false;
            bool subscriptionActive = false;
            
            OpenWeatherApiClient? openWeatherApiClient = null;
            if (!string.IsNullOrEmpty(weatherApiKey))
            {
                openWeatherApiClient = new OpenWeatherApiClient(weatherApiKey, config.Retry);
            }
            
            // Run startup progress before initializing live display
            await ConsoleUI.ShowStartupProgress(
                async () => {
                    tsw6Connected = await _tsw6ApiClient.IsApiAvailableAsync();
                    return tsw6Connected;
                },
                async () => {
                    if (tsw6Connected)
                    {
                        subscriptionActive = await _tsw6ApiClient.RegisterSubscription();
                    }
                    return subscriptionActive;
                }
            );

            if (!tsw6Connected)
            {
                Logger.LogError("TSW6 HTTP Server isn't accessible, check TSW6 is running with the -HTTPAPI flag set.");
                AnsiConsole.MarkupLine("[red]✗[/] Cannot continue without TSW6 connection.");
                await Task.Delay(3000);
                return;
            }

            // Now initialize the live display UI
            _ui = new ConsoleUI();
            _ui.Initialize(config.Weather.UpdateThresholdKm);
            _ui.UpdateStatusChecks(tsw6Connected, apiKeysFound, subscriptionActive);

            var weatherController = new RealtimeWeatherController(_tsw6ApiClient, openWeatherApiClient, config, _ui);
            await weatherController.InitialiseAsync();

            _ui.UpdateStatusChecks(tsw6Connected, apiKeysFound, subscriptionActive);

            // Main update loop - uses configured interval
            var intervalSeconds = config.Update.LocationCheckIntervalSeconds;
            Logger.LogInfo($"Starting weather update loop (every {intervalSeconds} seconds). Press Ctrl+C to exit.");
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
                    await weatherController.UpdatePlayerLocationAsync();
                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationTokenSource.Token);
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
