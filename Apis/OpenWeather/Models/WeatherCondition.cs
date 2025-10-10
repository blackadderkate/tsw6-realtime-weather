using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.OpenWeather.Models;

/// <summary>
/// Weather condition information
/// </summary>
public class WeatherCondition
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("main")]
    public string? Main { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}
