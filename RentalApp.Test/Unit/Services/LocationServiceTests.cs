/*
 * @file LocationServiceTests.cs
 * @brief Unit tests for LocationService using Moq to simulate GPS hardware behavior
 */

using Microsoft.Maui.Devices.Sensors;
using Moq;
using RentalApp.Services;

namespace RentalApp.Test.Unit.Services;

public class LocationServiceTests
{
    private readonly Mock<IGeolocation> _geolocationMock;
    private readonly LocationService _service;

    public LocationServiceTests()
    {
        _geolocationMock = new Mock<IGeolocation>();
        _service = new LocationService(_geolocationMock.Object);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_ShouldReturnCoordinates_WhenHardwareReturnsLocation()
    {
        // Arrange
        var expectedLat = 55.9331; // Napier
        var expectedLon = -3.2139;
        var mockLocation = new Location(expectedLat, expectedLon);

        _geolocationMock
            .Setup(g => g.GetLocationAsync(It.IsAny<GeolocationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLocation);

        // Act
        var result = await _service.GetCurrentLocationAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLat, result.Value.Latitude);
        Assert.Equal(expectedLon, result.Value.Longitude);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_ShouldReturnNull_WhenPermissionIsDenied()
    {
        // Arrange
        _geolocationMock
            .Setup(g => g.GetLocationAsync(It.IsAny<GeolocationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PermissionException("Denied"));

        // Act
        var result = await _service.GetCurrentLocationAsync();

        // Assert
        Assert.Null(result);
    }
}
