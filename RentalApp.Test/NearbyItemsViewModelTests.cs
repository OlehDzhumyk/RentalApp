/*
 * @file NearbyItemsViewModelTests.cs
 * @brief Unit tests for location-based item discovery
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Test.ViewModels;

/// <summary>
/// Unit tests for NearbyItemsViewModel. Verifies that the ViewModel 
/// correctly uses ILocationService and calls the repository for spatial queries.
/// </summary>
public class NearbyItemsViewModelTests
{
    private readonly Mock<IItemRepository> _itemRepositoryMock;
    private readonly Mock<ILocationService> _locationServiceMock;
    private readonly NearbyItemsViewModel _viewModel;

    public NearbyItemsViewModelTests()
    {
        _itemRepositoryMock = new Mock<IItemRepository>();
        _locationServiceMock = new Mock<ILocationService>();

        // RED STAGE: NearbyItemsViewModel does not exist yet.
        _viewModel = new NearbyItemsViewModel(_itemRepositoryMock.Object, _locationServiceMock.Object);
    }

    [Fact]
    public async Task LoadNearbyItemsCommand_ShouldUseCurrentLocation_AndFetchItems()
    {
        // Arrange: Simulate being in Edinburgh
        double edLat = 55.9533;
        double edLon = -3.1883;
        _locationServiceMock.Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync((edLat, edLon));

        _itemRepositoryMock.Setup(r => r.GetNearbyAsync(edLat, edLon, It.IsAny<double>()))
            .ReturnsAsync(new List<Item> { new Item { Id = 1, Title = "Nearby Drill" } });

        // Act
        await _viewModel.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        _itemRepositoryMock.Verify(r => r.GetNearbyAsync(edLat, edLon, 5.0), Times.Once);
        Assert.Single(_viewModel.NearbyItems);
    }

    [Fact]
    public async Task LoadNearbyItems_ShouldCalculateDistance_ForFoundItems()
    {
        // Arrange: User is at Edinburgh Castle
        double userLat = 55.9486;
        double userLon = -3.1999;
        _locationServiceMock.Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync((userLat, userLon));

        // Sample item at Waverley Station (~1km away)
        var nearbyItem = new Item { Id = 10, Title = "Station Drill", Location = new NetTopologySuite.Geometries.Point(-3.1883, 55.9533) { SRID = 4326 } };
        _itemRepositoryMock.Setup(r => r.GetNearbyAsync(userLat, userLon, It.IsAny<double>()))
            .ReturnsAsync(new List<Item> { nearbyItem });

        // Act
        await _viewModel.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Assert: Check if distance is exposed (we'll need a wrapper or a partial class logic)
        var firstResult = _viewModel.NearbyItems.First();
        Assert.NotNull(firstResult);
        // We expect the ViewModel to handle the distance calculation logic
    }


}