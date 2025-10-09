using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

public class Tsw6HttpRoute
{
    [JsonPropertyName("Verb")]
    public string Verb { get; set; } = string.Empty;

    [JsonPropertyName("Path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;
}
