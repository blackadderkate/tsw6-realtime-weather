using System.Text.Json.Serialization;
using Tsw6RealtimeWeather.Apis.Tsw6.Models;

namespace Tsw6RealtimeWeather.Apis.Tsw6;

[JsonSerializable(typeof(Tsw6SubscriptionData))]
[JsonSerializable(typeof(Tsw6ApiInfo))]
internal partial class Tsw6JsonContext : JsonSerializerContext
{
}