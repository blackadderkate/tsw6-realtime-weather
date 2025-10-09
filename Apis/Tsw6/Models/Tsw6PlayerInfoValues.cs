using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

public class Tsw6PlayerInfoValues
{
    [JsonPropertyName("geoLocation")]
    public Tsw6GeoLocation? GeoLocation { get; set; }

    [JsonPropertyName("currentTile")]
    public Tsw6CurrentTile? CurrentTile { get; set; }

    [JsonPropertyName("playerProfileName")]
    public string PlayerProfileName { get; set; } = string.Empty;

    [JsonPropertyName("cameraMode")]
    public string CameraMode { get; set; } = string.Empty;

    [JsonPropertyName("currentServiceName")]
    public string CurrentServiceName { get; set; } = string.Empty;
}
