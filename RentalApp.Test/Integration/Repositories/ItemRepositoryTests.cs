/*
 * @file ItemRepositoryTests.cs
 * @brief Integration tests for IItemRepository using real spatial data from Edinburgh
 */

using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
namespace RentalApp.Test.Integration.Repositories;

[Collection("Database collection")]
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
        var result = Assert.Single(results);
        Assert.Equal("Castle Drill", result.Item.Title);
        Assert.Equal("Owner", result.Item.Owner.FirstName);
        // Real distance is about 1.9 km; a flat "degrees x 111" estimate would give 2.3 km
        Assert.InRange(result.DistanceKm, 1.8, 2.0);
    }

    [Fact]
    public async Task GetNearbyAsync_ShouldOrderByDistance_AndSkipUnavailableOrUnlocatedItems()
    {
        var owner = new User { Email = "owner@napier.ac.uk", PasswordHash = "hash", FirstName = "Owner" };
        Context.Users.Add(owner);
        await Context.SaveChangesAsync();

        Context.Items.AddRange(
            new Item { Title = "Far", OwnerId = owner.Id, Location = _geometryFactory.CreatePoint(new Coordinate(-3.1883, 55.9533)) },
            new Item { Title = "Near", OwnerId = owner.Id, Location = _geometryFactory.CreatePoint(new Coordinate(-3.2100, 55.9340)) },
            new Item { Title = "Rented out", OwnerId = owner.Id, IsAvailable = false, Location = _geometryFactory.CreatePoint(new Coordinate(-3.2139, 55.9331)) },
            new Item { Title = "No location", OwnerId = owner.Id });
        await Context.SaveChangesAsync();

        var results = await _repository.GetNearbyAsync(55.9331, -3.2139, 5.0);

        Assert.Equal(new[] { "Near", "Far" }, results.Select(r => r.Item.Title));
        Assert.True(results[0].DistanceKm < results[1].DistanceKm);
    }
}
