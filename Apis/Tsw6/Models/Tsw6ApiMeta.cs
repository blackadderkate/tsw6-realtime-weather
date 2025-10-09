using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

public class Tsw6ApiMeta
{
    [JsonPropertyName("Worker")]
    public string Worker { get; set; } = string.Empty;

    [JsonPropertyName("GameName")]
    public string GameName { get; set; } = string.Empty;

    [JsonPropertyName("GameBuildNumber")]
    public int GameBuildNumber { get; set; }

    [JsonPropertyName("APIVersion")]
    public int APIVersion { get; set; }

    [JsonPropertyName("GameInstanceID")]
    public string GameInstanceID { get; set; } = string.Empty;
}
