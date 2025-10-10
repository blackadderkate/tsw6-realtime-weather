using System.Text.Json.Serialization;
using Tsw6RealtimeWeather.Apis.OpenWeather.Models;

namespace Tsw6RealtimeWeather.Apis.OpenWeather;

[JsonSerializable(typeof(OpenWeatherResponse))]
[JsonSerializable(typeof(Coordinates))]
[JsonSerializable(typeof(WeatherCondition))]
[JsonSerializable(typeof(MainWeatherData))]
[JsonSerializable(typeof(WindData))]
[JsonSerializable(typeof(PrecipitationData))]
[JsonSerializable(typeof(CloudData))]
[JsonSerializable(typeof(SystemData))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class OpenWeatherJsonContext : JsonSerializerContext
{
}
