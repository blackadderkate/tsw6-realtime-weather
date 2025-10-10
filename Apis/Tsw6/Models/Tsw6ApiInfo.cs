using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

public class Tsw6ApiInfo
{
    [JsonPropertyName("Meta")]
    public Tsw6ApiMeta Meta { get; set; } = null!;

    [JsonPropertyName("HttpRoutes")]
    public List<Tsw6HttpRoute> HttpRoutes { get; set; } = new();
}
