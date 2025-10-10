using NUnit.Framework;
using Tsw6RealtimeWeather.Weather;

namespace Tests;

/// <summary>
/// Unit tests for the PlayerLocation class, specifically testing distance calculations
/// using the Haversine formula
/// </summary>
[TestFixture]
public class PlayerLocationTests
{
    [Test]
    public void DistanceToInMeters_SameLocation_ReturnsZero()
    {
        // Arrange
        var location1 = new PlayerLocation(51.5074, -0.1278); // London
        var location2 = new PlayerLocation(51.5074, -0.1278); // Same location

        // Act
        var distance = location1.DistanceToInMeters(location2);

        // Assert
        Assert.That(distance, Is.EqualTo(0).Within(0.1), "Distance between identical locations should be zero");
    }

    [Test]
    public void DistanceToInMeters_LondonToNewYork_ReturnsCorrectDistance()
    {
        // Arrange
        var london = new PlayerLocation(51.5074, -0.1278);
        var newYork = new PlayerLocation(40.7128, -74.0060);

        // Act
        var distanceMeters = london.DistanceToInMeters(newYork);
        var distanceKm = distanceMeters / 1000.0;

        // Assert
        // The actual distance between London and New York is approximately 5,570 km
        Assert.That(distanceKm, Is.EqualTo(5570).Within(10), 
            "Distance from London to New York should be approximately 5,570 km");
    }

    [Test]
    public void DistanceToInKilometers_LondonToNewYork_ReturnsCorrectDistance()
    {
        // Arrange
        var london = new PlayerLocation(51.5074, -0.1278);
        var newYork = new PlayerLocation(40.7128, -74.0060);

        // Act
        var distanceKm = london.DistanceToInKilometers(newYork);

        // Assert
        Assert.That(distanceKm, Is.EqualTo(5570).Within(10), 
            "Distance from London to New York should be approximately 5,570 km");
    }

    [Test]
    public void DistanceToInMeters_ShortDistance_ReturnsAccurateResult()
    {
        // Arrange
        // Two points approximately 1 km apart
        var point1 = new PlayerLocation(51.5074, -0.1278); // London Eye
        var point2 = new PlayerLocation(51.5155, -0.1410); // Paddington Station

        // Act
        var distanceMeters = point1.DistanceToInMeters(point2);

        // Assert
        // Approximate distance is about 1,280 meters
        Assert.That(distanceMeters, Is.EqualTo(1280).Within(10), 
            "Distance should be approximately 1,280 meters");
    }

    [Test]
    public void DistanceToInMeters_VeryShortDistance_ReturnsAccurateResult()
    {
        // Arrange
        // Two points approximately 100 meters apart
        var point1 = new PlayerLocation(51.5074, -0.1278);
        var point2 = new PlayerLocation(51.5083, -0.1278); // About 100m north

        // Act
        var distanceMeters = point1.DistanceToInMeters(point2);

        // Assert
        Assert.That(distanceMeters, Is.EqualTo(130).Within(10), 
            "Distance should be approximately 130 meters");
    }

    [Test]
    public void DistanceToInMeters_EquatorToNorthPole_ReturnsQuarterEarthCircumference()
    {
        // Arrange
        var equator = new PlayerLocation(0, 0);
        var northPole = new PlayerLocation(90, 0);

        // Act
        var distanceKm = equator.DistanceToInKilometers(northPole);

        // Assert
        Assert.That(distanceKm, Is.EqualTo(10007).Within(10), 
            "Distance from equator to north pole should be approximately 10,007 km");
    }

    [Test]
    public void DistanceToInMeters_IsSymmetric()
    {
        // Arrange
        var location1 = new PlayerLocation(51.5074, -0.1278); // London
        var location2 = new PlayerLocation(48.8566, 2.3522);  // Paris

        // Act
        var distance1to2 = location1.DistanceToInMeters(location2);
        var distance2to1 = location2.DistanceToInMeters(location1);

        // Assert
        Assert.That(distance1to2, Is.EqualTo(distance2to1).Within(0.1), 
            "Distance calculation should be symmetric (A to B = B to A)");
    }

    [Test]
    public void DistanceToInMeters_AcrossDateLine_CalculatesCorrectly()
    {
        // Arrange
        var pointWest = new PlayerLocation(0, -179);  // Just west of date line
        var pointEast = new PlayerLocation(0, 179);   // Just east of date line

        // Act
        var distanceKm = pointWest.DistanceToInKilometers(pointEast);

        // Assert
        // Should be about 222 km (2 degrees at the equator)
        Assert.That(distanceKm, Is.EqualTo(222).Within(20), 
            "Distance across date line should be calculated correctly");
    }

    [Test]
    public void DistanceToInKilometers_ConvertsMetersCorrectly()
    {
        // Arrange
        var location1 = new PlayerLocation(51.5074, -0.1278);
        var location2 = new PlayerLocation(51.5174, -0.1278); // 1.11 km north

        // Act
        var distanceMeters = location1.DistanceToInMeters(location2);
        var distanceKm = location1.DistanceToInKilometers(location2);

        // Assert
        Assert.That(distanceKm, Is.EqualTo(distanceMeters / 1000.0).Within(0.001), 
            "Kilometers should equal meters divided by 1000");
    }

    [Test]
    public void DistanceToInMeters_NegativeLatitudes_CalculatesCorrectly()
    {
        // Arrange
        var sydney = new PlayerLocation(-33.8688, 151.2093);  // Sydney, Australia
        var melbourne = new PlayerLocation(-37.8136, 144.9631); // Melbourne, Australia

        // Act
        var distanceKm = sydney.DistanceToInKilometers(melbourne);

        // Assert
        // Approximate distance is about 714 km
        Assert.That(distanceKm, Is.EqualTo(714).Within(50), 
            "Distance between Sydney and Melbourne should be approximately 714 km");
    }

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var location = new PlayerLocation(51.507351, -0.127758);

        // Act
        var result = location.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("Lat=51.507351, Lon=-0.127758"), 
            "ToString should format with 6 decimal places");
    }

    [Test]
    public void Equals_SameCoordinates_ReturnsTrue()
    {
        // Arrange
        var location1 = new PlayerLocation(51.5074, -0.1278);
        var location2 = new PlayerLocation(51.5074, -0.1278);

        // Act & Assert
        Assert.That(location1.Equals(location2), Is.True, 
            "Locations with same coordinates should be equal");
    }

    [Test]
    public void Equals_DifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var location1 = new PlayerLocation(51.5074, -0.1278);
        var location2 = new PlayerLocation(48.8566, 2.3522);

        // Act & Assert
        Assert.That(location1.Equals(location2), Is.False, 
            "Locations with different coordinates should not be equal");
    }

    [Test]
    public void Default_ReturnsZeroCoordinates()
    {
        // Act
        var defaultLocation = PlayerLocation.Default();

        // Assert
        Assert.That(defaultLocation.Latitude, Is.EqualTo(0), "Default latitude should be 0");
        Assert.That(defaultLocation.Longitude, Is.EqualTo(0), "Default longitude should be 0");
    }
}
