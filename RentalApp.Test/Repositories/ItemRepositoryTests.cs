/*
 * @file ItemRepositoryTests.cs
 * @brief Unit tests for ItemRepository operations with proper setup
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;

namespace RentalApp.Test.Repositories;

public class ItemRepositoryTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new AppDbContext(options);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    [Fact]
    public async Task AddAsync_ShouldAddItemToDatabase()
    {
        // Arrange
        var context = GetDatabaseContext();
        IItemRepository repository = new ItemRepository(context);

        // We must create an owner first to satisfy foreign key constraints/logic
        var owner = new User
        {
            Id = 1,
            FirstName = "Owner",
            LastName = "User",
            Email = "owner@test.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        context.Users.Add(owner);
        await context.SaveChangesAsync();

        var newItem = new Item
        {
            Title = "Drill",
            Description = "Power drill",
            PricePerDay = 10.5m,
            OwnerId = owner.Id,
            IsAvailable = true // Explicitly setting state
        };

        // Act
        await repository.AddAsync(newItem);
        var items = await repository.GetAllAsync();

        // Assert
        Assert.Single(items);
        Assert.Equal("Drill", items[0].Title);
        Assert.Equal(owner.Id, items[0].OwnerId);
    }
}