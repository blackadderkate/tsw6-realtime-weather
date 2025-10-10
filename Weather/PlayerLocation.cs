namespace Tsw6RealtimeWeather.Weather;

/// <summary>
/// Represents a player's geographic location
/// </summary>
public class PlayerLocation
{
    /// <summary>
    /// The latitude coordinate in decimal degrees
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// The longitude coordinate in decimal degrees
    /// </summary>
    public double Longitude { get; set; }

    public PlayerLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Returns a string representation of the location
    /// </summary>
    public override string ToString()
    {
        return $"Lat={Latitude:F6}, Lon={Longitude:F6}";
    }

    /// <summary>
    /// Checks if two locations are equal (within a small tolerance for floating point comparison)
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is PlayerLocation other)
        {
            const double tolerance = 0.000001; // ~0.1 meters
            return Math.Abs(Latitude - other.Latitude) < tolerance &&
                   Math.Abs(Longitude - other.Longitude) < tolerance;
        }
        return false;
    }

    public static PlayerLocation Default()
    {
        return new PlayerLocation(0, 0);
    }

    /// <summary>
    /// Calculates the distance to another location using the Haversine formula
    /// </summary>
    /// <param name="other">The other location to calculate distance to</param>
    /// <returns>Distance in meters</returns>
    public double DistanceToInMeters(PlayerLocation other)
    {
        const double earthRadiusMeters = 6371000; // Earth's radius in meters
        
        // Convert degrees to radians
        var lat1Rad = DegreesToRadians(Latitude);
        var lat2Rad = DegreesToRadians(other.Latitude);
        var deltaLatRad = DegreesToRadians(other.Latitude - Latitude);
        var deltaLonRad = DegreesToRadians(other.Longitude - Longitude);

        // Haversine formula
        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return earthRadiusMeters * c;
    }

    /// <summary>
    /// Calculates the distance to another location in kilometers
    /// </summary>
    /// <param name="other">The other location to calculate distance to</param>
    /// <returns>Distance in kilometers</returns>
    public double DistanceToInKilometers(PlayerLocation other)
    {
        return DistanceToInMeters(other) / 1000.0;
    }

    /// <summary>
    /// Converts degrees to radians
    /// </summary>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude);
    }
}
