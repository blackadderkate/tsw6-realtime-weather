using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.OpenWeather.Models;

/// <summary>
/// Root response from OpenWeather Current Weather API
/// </summary>
public class OpenWeatherResponse
{
    [JsonPropertyName("coord")]
    public Coordinates? Coord { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherCondition>? Weather { get; set; }

    [JsonPropertyName("base")]
    public string? Base { get; set; }

    [JsonPropertyName("main")]
    public MainWeatherData? Main { get; set; }

    [JsonPropertyName("visibility")]
    public int? Visibility { get; set; }

    [JsonPropertyName("wind")]
    public WindData? Wind { get; set; }

    [JsonPropertyName("rain")]
    public PrecipitationData? Rain { get; set; }

    [JsonPropertyName("snow")]
    public PrecipitationData? Snow { get; set; }

    [JsonPropertyName("clouds")]
    public CloudData? Clouds { get; set; }

    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("sys")]
    public SystemData? Sys { get; set; }

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("cod")]
    public int Cod { get; set; }
}
