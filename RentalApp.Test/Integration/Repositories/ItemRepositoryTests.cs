/*
 * @file ItemRepositoryTests.cs
 * @brief Integration tests for IItemRepository using real spatial data from Edinburgh
 */

using NetTopologySuite.Geometries;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;

namespace RentalApp.Test.Integration.Repositories;

public class ItemRepositoryTests : BaseIntegrationTest
{
    private readonly IItemRepository _repository;
    private readonly GeometryFactory _geometryFactory;

    public ItemRepositoryTests()
    {
        _repository = ServiceProvider.GetRequiredService<IItemRepository>();
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    [Fact]
    public async Task GetNearbyAsync_ShouldOnlyReturnItemsInEdinburgh_WhenSearchingFromNapier()
    {
        // Arrange
        var owner = new User { Email = "owner@napier.ac.uk", PasswordHash = "hash", FirstName = "Owner" };
        Context.Users.Add(owner);
        await Context.SaveChangesAsync();

        // 1. Edinburgh Castle (~1.5km from Napier Merchiston)
        var edinburghItem = new Item
        {
            Title = "Castle Drill",
            OwnerId = owner.Id,
            Location = _geometryFactory.CreatePoint(new Coordinate(-3.1999, 55.9486))
        };

        // 2. Glasgow George Square (~70km from Napier)
        var glasgowItem = new Item
        {
            Title = "Glasgow Saw",
            OwnerId = owner.Id,
            Location = _geometryFactory.CreatePoint(new Coordinate(-4.2518, 55.8642))
        };

        Context.Items.AddRange(edinburghItem, glasgowItem);
        await Context.SaveChangesAsync();

        // Act: Search within 5km of Napier Merchiston (55.9331, -3.2139)
        var results = await _repository.GetNearbyAsync(55.9331, -3.2139, 5.0);

        // Assert
        Assert.Single(results);
        Assert.Equal("Castle Drill", results[0].Title);
        Assert.DoesNotContain(results, i => i.Title == "Glasgow Saw");
    }
}
