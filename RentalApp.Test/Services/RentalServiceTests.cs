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

        // RED STAGE: RentalService and IRentalRepository do not exist yet.
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
}
