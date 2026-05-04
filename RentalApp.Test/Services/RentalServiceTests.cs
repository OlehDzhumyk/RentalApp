/*
 * @file RentalServiceTests.cs
 * @brief Unit tests for rental business logic and overlap validation
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;

namespace RentalApp.Test.Services;

public class RentalServiceTests
{
    private readonly Mock<IRentalRepository> _rentalRepoMock;
    private readonly Mock<IItemRepository> _itemRepoMock;
    private readonly RentalService _rentalService;

    public RentalServiceTests()
    {
        _rentalRepoMock = new Mock<IRentalRepository>();
        _itemRepoMock = new Mock<IItemRepository>();

        _rentalService = new RentalService(_rentalRepoMock.Object, _itemRepoMock.Object);
    }

    [Fact]
    public async Task CanRentItem_ShouldReturnFalse_WhenDatesOverlapWithApprovedRental()
    {
        // Arrange
        int itemId = 1;
        var existingRental = new Rental
        {
            ItemId = itemId,
            StartDate = new DateTime(2026, 6, 10),
            EndDate = new DateTime(2026, 6, 15),
            Status = "Approved"
        };

        _rentalRepoMock.Setup(r => r.GetByItemIdAsync(itemId))
            .ReturnsAsync(new List<Rental> { existingRental });

        // Act: Try to rent from June 12 to June 14 (overlap)
        bool result = await _rentalService.CanRentItemAsync(itemId, new DateTime(2026, 6, 12), new DateTime(2026, 6, 14));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RequestRental_ShouldCalculateCorrectPrice()
    {
        // Arrange
        var itemId = 1;
        var item = new Item { Id = itemId, PricePerDay = 10.0m };
        var start = new DateTime(2026, 7, 1);
        var end = new DateTime(2026, 7, 4);

        _itemRepoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
        _rentalRepoMock.Setup(repo => repo.GetByItemIdAsync(itemId)).ReturnsAsync(new List<Rental>());
        _rentalRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Rental>())).ReturnsAsync((Rental r) => r);

        // Act
        var result = await _rentalService.RequestRentalAsync(itemId, 99, start, end);

        // Assert
        Assert.Equal(30.0m, result.TotalPrice);
        Assert.Equal("Requested", result.Status);
    }

}
