using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

/// <summary>
/// Weather data for TSW6 API
/// </summary>
public class Tsw6WeatherData
{
    /// <summary>
    /// Temperature in degrees Celsius
    /// </summary>
    [JsonPropertyName("Temperature")]
    public double Temperature { get; set; }

    /// <summary>
    /// Cloudiness level from 0 (clear) to 1 (overcast)
    /// </summary>
    [JsonPropertyName("Cloudiness")]
    public double Cloudiness { get; set; }

    /// <summary>
    /// Precipitation intensity from 0 (none) to 1 (heavy)
    /// </summary>
    [JsonPropertyName("Precipitation")]
    public double Precipitation { get; set; }

    /// <summary>
    /// Ground wetness from 0 (dry) to 1 (wet)
    /// </summary>
    [JsonPropertyName("Wetness")]
    public double Wetness { get; set; }

    /// <summary>
    /// Snow on ground from 0 (none) to 1 (heavy)
    /// </summary>
    [JsonPropertyName("GroundSnow")]
    public double GroundSnow { get; set; }

    /// <summary>
    /// Fog density from 0 (clear) to 1 (dense), typically 0-0.1
    /// </summary>
    [JsonPropertyName("FogDensity")]
    public double FogDensity { get; set; }
}
