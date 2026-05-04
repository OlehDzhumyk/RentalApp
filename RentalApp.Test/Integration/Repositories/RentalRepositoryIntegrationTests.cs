/*
 * @file RentalRepositoryIntegrationTests.cs
 * @brief Integration tests for IRentalRepository contract using real database
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;
using RentalApp.Database.Repositories;

namespace RentalApp.Test.Integration.Repositories;

public class RentalRepositoryIntegrationTests : BaseIntegrationTest
{
    private readonly IRentalRepository _repository;

    public RentalRepositoryIntegrationTests()
    {
        _repository = ServiceProvider.GetRequiredService<IRentalRepository>();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistRental_WithCorrectDates()
    {
        // Arrange
        var owner = new User { Email = "owner_r@test.com", FirstName = "Owner", PasswordHash = "hash" };
        var borrower = new User { Email = "borrower_r@test.com", FirstName = "Borrower", PasswordHash = "hash" };
        Context.Users.AddRange(owner, borrower);
        await Context.SaveChangesAsync();

        var item = new Item { Title = "Impact Driver", OwnerId = owner.Id, PricePerDay = 12.0m };
        Context.Items.Add(item);
        await Context.SaveChangesAsync();

        var rental = new Rental
        {
            ItemId = item.Id,
            BorrowerId = borrower.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            TotalPrice = 24.0m,
            Status = "Requested"
        };

        // Act
        var created = await _repository.CreateAsync(rental);

        // Assert
        var dbRecord = await _repository.GetByIdAsync(created.Id);
        Assert.NotNull(dbRecord);
        Assert.Equal(rental.TotalPrice, dbRecord.TotalPrice);
        Assert.Equal("Requested", dbRecord.Status);
    }

    [Fact]
    public async Task GetByItemIdAsync_ShouldReturnAllRentalsForItem()
    {
        // Arrange
        var user = new User { Email = "multi@test.com", FirstName = "Test", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var item = new Item { Title = "Ladder", OwnerId = user.Id, PricePerDay = 5.0m };
        Context.Items.Add(item);
        await Context.SaveChangesAsync();

        // Seed two rentals for the same item
        Context.Rentals.AddRange(
            new Rental { ItemId = item.Id, BorrowerId = user.Id, Status = "Approved", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1) },
            new Rental { ItemId = item.Id, BorrowerId = user.Id, Status = "Completed", StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(-4) }
        );
        await Context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByItemIdAsync(item.Id);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistStatusChange()
    {
        // Arrange
        var user = new User { Email = "update@test.com", FirstName = "User", PasswordHash = "hash" };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var item = new Item { Title = "Saw", OwnerId = user.Id, PricePerDay = 10.0m };
        Context.Items.Add(item);
        await Context.SaveChangesAsync();

        var rental = new Rental { ItemId = item.Id, BorrowerId = user.Id, Status = "Requested" };
        await _repository.CreateAsync(rental);

        // Act
        rental.Status = "Approved";
        await _repository.UpdateAsync(rental);

        // Assert
        var updated = await _repository.GetByIdAsync(rental.Id);
        Assert.Equal("Approved", updated?.Status);
    }
}
