using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.OpenWeather.Models;

/// <summary>
/// Cloud coverage data
/// </summary>
public class CloudData
{
    /// <summary>
    /// Cloudiness percentage
    /// </summary>
    [JsonPropertyName("all")]
    public int All { get; set; }
}
