using System;
using System.Threading.Tasks;
using Spectre.Console;

namespace Tsw6RealtimeWeather.UI;

/// <summary>
/// Manages the Spectre.Console UI for displaying application status
/// </summary>
public class ConsoleUI
{
    private Layout? _layout;
    private Table? _statusTable;
    private Panel? _distancePanel;
    private Panel? _weatherPanel;
    
    private double _accumulatedDistance = 0.0;
    private double _distanceThreshold = 10.0;
    private string _currentLocation = "Unknown";
    private string _weatherInfo = "Waiting for first update...";
    
    private bool _tsw6Connected = false;
    private bool _apiKeysFound = false;
    private bool _subscriptionActive = false;

    public void Initialize(double distanceThreshold)
    {
        _distanceThreshold = distanceThreshold;
        AnsiConsole.Clear();
        
        // Display header
        AnsiConsole.Write(
            new FigletText("TSW6 Weather")
                .LeftJustified()
                .Color(Color.Cyan1));
        
        AnsiConsole.MarkupLine("[dim]Real-time weather synchronization for Train Sim World 6[/]\n");
        
        // Create the layout
        _layout = new Layout("Root")
            .SplitRows(
                new Layout("Status"),
                new Layout("Main").SplitColumns(
                    new Layout("Distance"),
                    new Layout("Weather")
                )
            );
        
        // Set proportions
        _layout["Status"].Size(10);
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

        // Create status table
        _statusTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn(new TableColumn("Status Check").Centered())
            .AddColumn(new TableColumn("Result").Centered());

        _statusTable.AddRow(
            "TSW6 Connection",
            _tsw6Connected ? "[green]✓ Connected[/]" : "[red]✗ Disconnected[/]"
        );
        
        _statusTable.AddRow(
            "API Keys",
            _apiKeysFound ? "[green]✓ Found[/]" : "[yellow]⚠ Missing[/]"
        );
        
        _statusTable.AddRow(
            "Subscription",
            _subscriptionActive ? "[green]✓ Active[/]" : "[grey]⊙ Inactive[/]"
        );

        // Create distance panel with progress bar
        var distancePercentage = Math.Min((_accumulatedDistance / _distanceThreshold) * 100.0, 100.0);
        
        // Create a visual progress bar using BreakdownChart
        var progressChart = new BreakdownChart()
            .Width(60)
            .ShowPercentage()
            .UseValueFormatter(value => $"{value:F2}%")
            .AddItem("Travelled", distancePercentage, Color.Green)
            .AddItem("Remaining", 100.0 - distancePercentage, Color.Grey);

        var distanceContent = new Rows(
            new Markup($"[bold]Current Location:[/] {Markup.Escape(_currentLocation)}"),
            new Text(""),
            new Markup($"[bold]Distance Travelled:[/] {_accumulatedDistance:F2} km ({distancePercentage:F2}%)"),
            new Markup($"[bold]Next Update At:[/] {_distanceThreshold:F2} km (100.00%)"),
            new Text(""),
            progressChart
        );

        _distancePanel = new Panel(distanceContent)
            .Header("[yellow]Distance Tracking[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow);

        // Create weather panel
        var weatherContent = new Markup($"[dim]{Markup.Escape(_weatherInfo)}[/]");
        
        _weatherPanel = new Panel(weatherContent)
            .Header("[cyan]Weather Information[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan1);

        // Update layout
        _layout["Status"].Update(
            new Panel(_statusTable)
                .Header("[green]System Status[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
        );
        
        _layout["Distance"].Update(_distancePanel);
        _layout["Weather"].Update(_weatherPanel);

        // Render the layout
        AnsiConsole.Clear();
        
        // Re-display header
        AnsiConsole.Write(
            new FigletText("TSW6 Weather")
                .LeftJustified()
                .Color(Color.Cyan1));
        
        AnsiConsole.MarkupLine("[dim]Real-time weather synchronization for Train Sim World 6[/]\n");
        
        AnsiConsole.Write(_layout);
        
        AnsiConsole.MarkupLine("\n[dim]Press Ctrl+C to exit[/]");
    }

    public async Task ShowStartupProgress(Func<Task<bool>> tsw6Check, Func<Task<bool>> subscriptionSetup)
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
