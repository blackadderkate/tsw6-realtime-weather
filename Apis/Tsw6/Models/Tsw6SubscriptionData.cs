using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tsw6RealtimeWeather.Apis.Tsw6.Models;

public class Tsw6SubscriptionData
{
    [JsonPropertyName("RequestedSubscriptionID")]
    public int RequestedSubscriptionID { get; set; }

    [JsonPropertyName("Entries")]
    public List<Tsw6SubscriptionEntry> Entries { get; set; } = new();
}
