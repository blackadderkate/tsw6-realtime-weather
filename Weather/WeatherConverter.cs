using System;
using Tsw6RealtimeWeather.Apis.OpenWeather.Models;
using Tsw6RealtimeWeather.Apis.Tsw6.Models;

namespace Tsw6RealtimeWeather.Weather;

/// <summary>
/// Converts OpenWeather API data to TSW6 weather format
/// </summary>
public static class WeatherConverter
{
    /// <summary>
    /// Converts OpenWeather API response to TSW6 weather data
    /// </summary>
    public static Tsw6WeatherData ConvertToTsw6Weather(OpenWeatherResponse openWeather)
    {
        var tsw6Weather = new Tsw6WeatherData();

        // Temperature - direct conversion from Kelvin to Celsius
        if (openWeather.Main != null)
        {
            tsw6Weather.Temperature = openWeather.Main.Temp - 273.15;
        }

        // Cloudiness - convert from 0-100 percentage to 0-1
        if (openWeather.Clouds != null)
        {
            tsw6Weather.Cloudiness = openWeather.Clouds.All / 100.0;
        }

        // Precipitation - calculate from rain and snow intensity
        // OpenWeather gives mm/h, we need to map to 0-1 scale
        // Assume: 0mm = 0, 10mm/h+ = 1 (heavy rain/snow)
        double precipitationMmh = 0;
        
        if (openWeather.Rain?.OneHour.HasValue == true)
        {
            precipitationMmh += openWeather.Rain.OneHour.Value;
        }
        
        if (openWeather.Snow?.OneHour.HasValue == true)
        {
            precipitationMmh += openWeather.Snow.OneHour.Value;
        }
        
        tsw6Weather.Precipitation = Math.Clamp(precipitationMmh / 10.0, 0, 1);

        // Wetness - based on recent precipitation and humidity
        // High humidity + precipitation = wet ground
        // Formula: weighted average of precipitation and humidity
        double humidityFactor = openWeather.Main?.Humidity / 100.0 ?? 0;
        tsw6Weather.Wetness = Math.Clamp(
            (tsw6Weather.Precipitation * 0.7) + (humidityFactor * 0.3),
            0, 
            1
        );

        // GroundSnow - only set if there's active snow or snow weather condition
        // Check weather condition ID (6xx range is snow)
        bool isSnowing = openWeather.Weather?.Count > 0 && 
                        openWeather.Weather[0].Id >= 600 && 
                        openWeather.Weather[0].Id < 700;
        
        if (isSnowing && openWeather.Snow?.OneHour.HasValue == true)
        {
            // Map snow intensity to ground coverage
            // 0mm = 0, 5mm/h+ = 1 (heavy snow coverage)
            tsw6Weather.GroundSnow = Math.Clamp(openWeather.Snow.OneHour.Value / 5.0, 0, 1);
        }
        else
        {
            tsw6Weather.GroundSnow = 0;
        }

        // FogDensity - map from visibility
        // Under 1km = max fog (0.1), 10km+ = clear (0)
        if (openWeather.Visibility.HasValue)
        {
            double visibilityKm = openWeather.Visibility.Value / 1000.0;
            
            if (visibilityKm >= 10)
            {
                tsw6Weather.FogDensity = 0;
            }
            else if (visibilityKm <= 1)
            {
                tsw6Weather.FogDensity = 0.1;
            }
            else
            {
                // Linear interpolation between 1km (0.1) and 10km (0)
                tsw6Weather.FogDensity = 0.1 * (1 - ((visibilityKm - 1) / 9.0));
            }
        }
        else
        {
            tsw6Weather.FogDensity = 0; // Default to clear if no visibility data
        }

        Logger.LogDebug($"Weather conversion: Temp={tsw6Weather.Temperature:F1}°C, " +
                       $"Cloud={tsw6Weather.Cloudiness:F2}, Precip={tsw6Weather.Precipitation:F2}, " +
                       $"Wet={tsw6Weather.Wetness:F2}, Snow={tsw6Weather.GroundSnow:F2}, " +
                       $"Fog={tsw6Weather.FogDensity:F3}");

        return tsw6Weather;
    }
}
