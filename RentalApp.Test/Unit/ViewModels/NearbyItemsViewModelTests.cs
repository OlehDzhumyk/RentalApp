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
            .ReturnsAsync(new List<NearbyItem> { new(new Item { Id = 1, Title = "Nearby Drill" }, 0.4) });

        // Act
        await _viewModel.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        _itemRepositoryMock.Verify(r => r.GetNearbyAsync(edLat, edLon, 5.0), Times.Once);
        var result = Assert.Single(_viewModel.NearbyItems);
        Assert.Equal("Nearby Drill", result.Item.Title);
        Assert.Equal(0.4, result.DistanceKm);
    }

    [Fact]
    public async Task LoadNearbyItems_ShouldUseSearchRadius()
    {
        _locationServiceMock.Setup(l => l.GetCurrentLocationAsync()).ReturnsAsync((55.9, -3.2));
        _itemRepositoryMock.Setup(r => r.GetNearbyAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new List<NearbyItem>());
        _viewModel.SearchRadius = 12;

        await _viewModel.LoadNearbyItemsCommand.ExecuteAsync(null);

        _itemRepositoryMock.Verify(r => r.GetNearbyAsync(55.9, -3.2, 12), Times.Once);
    }

    [Fact]
    public async Task LoadNearbyItems_ShouldShowError_WhenLocationIsUnavailable()
    {
        _locationServiceMock.Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync(((double, double)?)null);

        await _viewModel.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasError);
        Assert.Contains("location", _viewModel.ErrorMessage);
        _itemRepositoryMock.Verify(r => r.GetNearbyAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task LoadNearbyItems_ShouldShowError_WhenRepositoryFails()
    {
        _locationServiceMock.Setup(l => l.GetCurrentLocationAsync()).ReturnsAsync((55.9, -3.2));
        _itemRepositoryMock.Setup(r => r.GetNearbyAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ThrowsAsync(new InvalidOperationException("connection refused"));

        await _viewModel.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasError);
        Assert.Contains("connection refused", _viewModel.ErrorMessage);
        Assert.False(_viewModel.IsBusy);
    }
}
