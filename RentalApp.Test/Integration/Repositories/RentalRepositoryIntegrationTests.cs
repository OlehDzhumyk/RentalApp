/*
 * @file RentalRepositoryIntegrationTests.cs
 * @brief Integration tests for RentalRepository using a real database
 */

using RentalApp.Database.Models;
using RentalApp.Database.Repositories;

namespace RentalApp.Test.Integration.Repositories;

public class RentalRepositoryIntegrationTests : BaseIntegrationTest
{
    private readonly RentalRepository _repository;

    public RentalRepositoryIntegrationTests()
    {
        _repository = new RentalRepository(Context);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistRentalInDatabase()
    {
        // Arrange
        var owner = new User { Email = "owner@test.com", PasswordHash = "hash", FirstName = "Owner" };
        var borrower = new User { Email = "borrower@test.com", PasswordHash = "hash", FirstName = "Borrower" };
        Context.Users.AddRange(owner, borrower);
        await Context.SaveChangesAsync();

        var item = new Item { Title = "Test Tool", OwnerId = owner.Id, PricePerDay = 10 };
        Context.Items.Add(item);
        await Context.SaveChangesAsync();

        var rental = new Rental
        {
            ItemId = item.Id,
            BorrowerId = borrower.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            TotalPrice = 20,
            Status = "Requested"
        };

        // Act
        var created = await _repository.CreateAsync(rental);

        // Assert
        var dbRental = await Context.Rentals.FindAsync(created.Id);
        Assert.NotNull(dbRental);
        Assert.Equal("Requested", dbRental.Status);
        Assert.Equal(item.Id, dbRental.ItemId);
    }
}
