using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

/// <summary>
/// Wrapper for single value PATCH requests to TSW6 API
/// </summary>
public class Tsw6ValueWrapper
{
    [JsonPropertyName("Value")]
    public double Value { get; set; }

    public Tsw6ValueWrapper(double value)
    {
        Value = value;
    }
}
