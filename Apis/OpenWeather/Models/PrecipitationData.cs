using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.OpenWeather.Models;

/// <summary>
/// Precipitation data (rain or snow)
/// Only present in response when raining or snowing
/// </summary>
public class PrecipitationData
{
    /// <summary>
    /// Precipitation volume for last 1 hour, mm
    /// </summary>
    [JsonPropertyName("1h")]
    public double? OneHour { get; set; }

    /// <summary>
    /// Precipitation volume for last 3 hours, mm
    /// </summary>
    [JsonPropertyName("3h")]
    public double? ThreeHours { get; set; }
}
