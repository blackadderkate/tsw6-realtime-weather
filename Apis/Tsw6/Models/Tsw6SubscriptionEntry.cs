using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

public class Tsw6SubscriptionEntry
{
    [JsonPropertyName("Path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("NodeValid")]
    public bool NodeValid { get; set; }

    [JsonPropertyName("Values")]
    public Tsw6PlayerInfoValues? Values { get; set; }
}
