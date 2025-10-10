using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.OpenWeather.Models;

/// <summary>
/// Geographic coordinates
/// </summary>
public class Coordinates
{
    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }
}
