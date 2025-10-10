using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spectre.Console;
using Tsw6RealtimeWeather.Apis.OpenWeather.Models;
using YamlDotNet.Core;

namespace Tsw6RealtimeWeather.UI;

/// <summary>
/// Manages the Spectre.Console UI for displaying application status
/// </summary>
public class ConsoleUI
{
    private Layout? _layout;
    private Panel? _distancePanel;
    private Panel? _weatherPanel;
    
    private double _accumulatedDistance = 0.0;
    private double _distanceThreshold = 10.0;
    private string _currentLocation = "Unknown";
    private string _weatherInfo = "Waiting for first update...";
    private OpenWeatherResponse? _currentWeather = null;
    
    private bool _tsw6Connected = false;
    private bool _apiKeysFound = false;
    private bool _subscriptionActive = false;

    public void Initialize(double distanceThreshold)
    {
        _distanceThreshold = distanceThreshold;
        AnsiConsole.Clear();
        
        // Display compact header
        AnsiConsole.Write(
            new FigletText("TSW6 Weather")
                .LeftJustified()
                .Color(Color.Cyan1));
        
        AnsiConsole.MarkupLine("[dim]Real-time weather sync for Train Sim World 6[/]\n");
        
        // Create the layout - only Distance and Weather columns
        _layout = new Layout("Root")
            .SplitColumns(
                new Layout("Distance"),
                new Layout("Weather")
            );
        
        // Set proportions
        _layout["Distance"].Size(50);
        _layout["Weather"].Size(50);
        
        UpdateDisplay();
    }

    public void UpdateStatusChecks(bool tsw6Connected, bool apiKeysFound, bool subscriptionActive)
    {
        _tsw6Connected = tsw6Connected;
        _apiKeysFound = apiKeysFound;
        _subscriptionActive = subscriptionActive;
        UpdateDisplay();
    }

    public void UpdateDistance(double accumulatedKm, string location)
    {
        _accumulatedDistance = accumulatedKm;
        _currentLocation = location;
        UpdateDisplay();
    }

    public void UpdateWeather(string weatherInfo)
    {
        _weatherInfo = weatherInfo;
        _currentWeather = null; // Clear structured data when using string
        UpdateDisplay();
    }

    public void UpdateWeather(OpenWeatherResponse? weather)
    {
        _currentWeather = weather;
        if (weather == null)
        {
            _weatherInfo = "⚠ Failed to fetch weather data";
        }
        else
        {
            _weatherInfo = ""; // Clear string when using structured data
        }
        UpdateDisplay();
    }

    public void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(message)}");
    }

    public void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(message)}");
    }

    public void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[blue]ℹ[/] {Markup.Escape(message)}");
    }

    private void UpdateDisplay()
    {
        if (_layout == null) return;

        // Show warnings only if there are issues
        List<string> warnings = new List<string>();
        
        if (!_tsw6Connected)
            warnings.Add("⚠ TSW6 not connected - Start TSW6 with -HTTPAPI flag");
        
        if (!_apiKeysFound)
            warnings.Add("⚠ API keys not found - Check config.yaml");
        
        if (!_subscriptionActive && _tsw6Connected)
            warnings.Add("⚠ Subscription not active - Drive the train to activate");

        // Create compact distance panel with progress bar
        var distancePercentage = Math.Min((_accumulatedDistance / _distanceThreshold) * 100.0, 100.0);
        
        // Create a visual progress bar using BreakdownChart
        var progressChart = new BreakdownChart()
            .Width(50)
            .ShowPercentage()
            .UseValueFormatter(value => $"{value:F1}%")
            .AddItem("Travelled", distancePercentage, Color.Green)
            .AddItem("Remaining", 100.0 - distancePercentage, Color.Grey);

        var distanceContent = new Rows(
            new Markup($"[bold]Location:[/] {Markup.Escape(_currentLocation)}"),
            progressChart
        );

        _distancePanel = new Panel(distanceContent)
            .Header("[yellow]Distance[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow);

        // Create weather panel
        Spectre.Console.Rendering.IRenderable weatherContent;
        
        if (_currentWeather != null)
        {
            // Structured weather display
            weatherContent = CreateWeatherDisplay(_currentWeather);
        }
        else if (!string.IsNullOrEmpty(_weatherInfo))
        {
            // Fallback to string display
            weatherContent = new Markup($"[dim]{Markup.Escape(_weatherInfo)}[/]");
        }
        else
        {
            weatherContent = new Markup("[dim]Waiting for first update...[/]");
        }
        
        _weatherPanel = new Panel(weatherContent)
            .Header("[cyan]Weather[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan1);

        // Update layout
        _layout["Distance"].Update(_distancePanel);
        _layout["Weather"].Update(_weatherPanel);

        // Render the layout
        AnsiConsole.Clear();
        
        // Re-display compact header
        AnsiConsole.Write(
            new FigletText("TSW6 Weather")
                .LeftJustified()
                .Color(Color.Cyan1));
        
        AnsiConsole.MarkupLine("[dim]Real-time weather sync for Train Sim World 6[/]\n");
        
        // Show warnings if there are any issues
        if (warnings.Count > 0)
        {
            foreach (var warning in warnings)
            {
                AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(warning)}[/]");
            }
            AnsiConsole.WriteLine();
        }
        
        AnsiConsole.Write(_layout);
        
        AnsiConsole.MarkupLine("\n[dim]Ctrl+C to exit[/]");
    }

    private Spectre.Console.Rendering.IRenderable CreateWeatherDisplay(OpenWeatherResponse weather)
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(1))
            .AddColumn(new GridColumn());

        // Location - make it more compact
        if (!string.IsNullOrEmpty(weather.Name))
        {
            grid.AddRow(
                new Markup("[bold]Location:[/]"),
                new Markup($"{Markup.Escape(weather.Name)}, {weather.Sys?.Country ?? "??"}")
            );
        }

        // Weather Condition
        if (weather.Weather != null && weather.Weather.Count > 0)
        {
            var condition = weather.Weather[0];
            var emoji = GetWeatherEmoji(condition.Id);
            grid.AddRow(
                new Markup($"[bold]Conditions:[/]"),
                new Markup($"{Markup.Escape(condition.Main ?? "Unknown")} - {Markup.Escape(condition.Description ?? "")}")
            );
        }

        // Temperature - color coded based on temperature
        if (weather.Main != null)
        {
            var tempC = weather.Main.Temp - 273.15;
            var tempColor = GetTemperatureColor(tempC);
            
            grid.AddRow(
                new Markup("[bold]Temperature[/]"),
                new Markup($"[{tempColor}]{tempC:F1}°C[/]")
            );
        }

        // Cloud Cover
        if (weather.Clouds != null)
        {
            var cloudColor = weather.Clouds.All switch
            {
                >= 80 => "grey",
                >= 50 => "silver",
                >= 20 => "white",
                _ => "cyan"
            };
            
            grid.AddRow(
                new Markup("[bold]Cloud cover:[/]"),
                new Markup($"[{cloudColor}]{weather.Clouds.All}%[/]")
            );
        }

        // Precipitation - compact
        if (weather.Rain != null && weather.Rain.OneHour.HasValue && weather.Rain.OneHour.Value > 0)
        {
            grid.AddRow(
                new Markup("[bold]Rain:[/]"),
                new Markup($"[blue]{weather.Rain.OneHour.Value:F1} mm/h[/]")
            );
        }
        else if (weather.Snow != null && weather.Snow.OneHour.HasValue && weather.Snow.OneHour.Value > 0)
        {
            grid.AddRow(
                new Markup("[bold]Snow:[/]"),
                new Markup($"[white]{weather.Snow.OneHour.Value:F1} mm/h[/]")
            );
        }

        // Wind - single line with gusts
        if (weather.Wind != null)
        {
            var windSpeedKmh = weather.Wind.Speed * 3.6;
            var windDir = GetWindDirection(weather.Wind.Deg);
            var windStrength = windSpeedKmh switch
            {
                >= 50 => "red",
                >= 30 => "orange1",
                >= 15 => "yellow",
                _ => "green"
            };
            
            var windText = $"[{windStrength}]{windSpeedKmh:F1} km/h {windDir}[/]";
            
            grid.AddRow(
                new Markup("[bold]Wind:[/]"),
                new Markup(windText)
            );
        }

        // Additional Info - single compact line
        if (weather.Main != null)
        {
           
            if (weather.Visibility.HasValue)
            {
                var visibilityKm = weather.Visibility.Value / 1000.0;
                var infoText = $"{visibilityKm:F1}km";

                grid.AddRow(
                new Markup("[bold]Visibility:[/]"),
                new Markup($"[dim]{infoText}[/]")
            );
            }
        }

        // Last Update - compact
        var lastUpdate = DateTimeOffset.FromUnixTimeSeconds(weather.Dt).ToLocalTime();
        grid.AddRow(
            new Markup("[bold]Last updated:[/]"),
            new Markup($"{lastUpdate:HH:mm:ss}")
        );

        return grid;
    }

    private string GetWeatherEmoji(int conditionId)
    {
        return conditionId switch
        {
            >= 200 and < 300 => "⛈️",  // Thunderstorm
            >= 300 and < 400 => "🌦️",  // Drizzle
            >= 500 and < 600 => "🌧️",  // Rain
            >= 600 and < 700 => "❄️",  // Snow
            >= 700 and < 800 => "🌫️",  // Atmosphere (mist, fog, etc.)
            800 => "☀️",                // Clear
            >= 801 and < 900 => "☁️",  // Clouds
            _ => "🌤️"                  // Default
        };
    }

    private string GetWindDirection(int degrees)
    {
        var directions = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        var index = (int)Math.Round(((degrees % 360) / 45.0)) % 8;
        return directions[index];
    }

    private static string GetTemperatureColor(double tempC)
    {
        return tempC switch
        {
            // Freezing and below - blue shades
            < -10 => "blue",           // Very cold: < -10°C
            < 0 => "dodgerblue1",      // Cold: -10°C to 0°C
            
            // Cool - cyan/teal shades
            < 10 => "cyan",            // Cool: 0°C to 10°C
            < 15 => "aqua",            // Mild cool: 10°C to 15°C
            
            // Comfortable - green shades
            < 20 => "green",           // Comfortable: 15°C to 20°C
            < 25 => "greenyellow",     // Warm: 20°C to 25°C
            
            // Hot - yellow/orange shades
            < 30 => "yellow",          // Hot: 25°C to 30°C
            < 35 => "orange1",         // Very hot: 30°C to 35°C
            
            // Extreme heat - red shades
            < 40 => "red",             // Extremely hot: 35°C to 40°C
            _ => "red1"                // Dangerously hot: 40°C+
        };
    }

    public static async Task ShowStartupProgress(Func<Task<bool>> tsw6Check, Func<Task<bool>> subscriptionSetup)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Starting up...", async ctx =>
            {
                ctx.Status("Checking for TSW6 API...");
                await Task.Delay(500);
                var tsw6Result = await tsw6Check();
                
                if (tsw6Result)
                {
                    AnsiConsole.MarkupLine("[green]✓[/] TSW6 API connected");
                    
                    ctx.Status("Setting up subscription...");
                    await Task.Delay(500);
                    var subResult = await subscriptionSetup();
                    
                    if (subResult)
                    {
                        AnsiConsole.MarkupLine("[green]✓[/] Subscription registered");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]✗[/] Failed to register subscription");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]✗[/] TSW6 API not available");
                }
                
                await Task.Delay(1000);
            });
    }
}
