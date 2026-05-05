/*
 * @file RentalServiceTests.cs
 * @brief Unit tests for rental business logic and state transitions
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Database.States;
using RentalApp.Services;
using Xunit;

namespace RentalApp.Test.Unit.Services;

/// <summary>
/// Verifies the integrity of rental processes, including availability checks, 
/// price calculations, and state-driven transitions.
/// </summary>
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

    #region Availability & Creation Tests

    [Fact]
    public async Task CanRentItem_ShouldReturnFalse_WhenDatesOverlap()
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

        // Act: Overlap exists (12th-14th is inside 10th-15th)
        bool result = await _rentalService.CanRentItemAsync(itemId, new DateTime(2026, 6, 12), new DateTime(2026, 6, 14));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RequestRental_ShouldCalculateCorrectPrice_ForMultiDayRental()
    {
        // Arrange
        var itemId = 1;
        var item = new Item { Id = itemId, PricePerDay = 15.0m };
        var start = new DateTime(2026, 7, 1);
        var end = new DateTime(2026, 7, 3); // 2 days

        _itemRepoMock.Setup(repo => repo.GetByIdAsync(itemId)).ReturnsAsync(item);
        _rentalRepoMock.Setup(repo => repo.GetByItemIdAsync(itemId)).ReturnsAsync(new List<Rental>());
        _rentalRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<Rental>())).ReturnsAsync((Rental r) => r);

        // Act
        var result = await _rentalService.RequestRentalAsync(itemId, 99, start, end);

        // Assert
        Assert.Equal(30.0m, result.TotalPrice);
        Assert.Equal("Requested", result.Status);
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task Transition_ShouldSucceed_WhenApprovingRequestedRental()
    {
        // Arrange
        int rentalId = 10;
        var rental = new Rental { Id = rentalId, Status = "Requested" };

        _rentalRepoMock.Setup(r => r.GetByIdAsync(rentalId)).ReturnsAsync(rental);

        // FIX: UpdateAsync returns Task, so we use Returns(Task.CompletedTask)
        _rentalRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Rental>()))
                       .Returns(Task.CompletedTask);

        // Act
        bool result = await _rentalService.TransitionAsync(rentalId, (state, r) => state.ApproveAsync(r));

        // Assert
        Assert.True(result);
        Assert.Equal("Approved", rental.Status);
        _rentalRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Rental>()), Times.Once);
    }

    [Fact]
    public async Task Transition_ShouldFail_WhenApprovingAlreadyApprovedRental()
    {
        // Arrange
        int rentalId = 11;
        var rental = new Rental { Id = rentalId, Status = "Approved" };

        _rentalRepoMock.Setup(r => r.GetByIdAsync(rentalId)).ReturnsAsync(rental);

        // Act
        bool result = await _rentalService.TransitionAsync(rentalId, (state, r) => state.ApproveAsync(r));

        // Assert
        Assert.False(result);
        Assert.Equal("Approved", rental.Status); // Status should not change
        _rentalRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Rental>()), Times.Never);
    }

    [Fact]
    public async Task Transition_ShouldSucceed_WhenRejectingRequestedRental()
    {
        // Arrange
        int rentalId = 12;
        var rental = new Rental { Id = rentalId, Status = "Requested" };

        _rentalRepoMock.Setup(r => r.GetByIdAsync(rentalId)).ReturnsAsync(rental);

        // Act
        bool result = await _rentalService.TransitionAsync(rentalId, (state, r) => state.RejectAsync(r));

        // Assert
        Assert.True(result);
        Assert.Equal("Rejected", rental.Status);
    }

    [Fact]
    public async Task Transition_ShouldReturnFalse_WhenRentalNotFound()
    {
        // Arrange
        _rentalRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Rental)null!);

        // Act
        bool result = await _rentalService.TransitionAsync(999, (state, r) => state.ApproveAsync(r));

        // Assert
        Assert.False(result);
    }

    #endregion
}
