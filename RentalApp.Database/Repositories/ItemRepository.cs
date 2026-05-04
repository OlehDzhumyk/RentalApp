/*
 * @file ItemRepository.cs
 * @brief Implementation of IItemRepository with PostGIS spatial support
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

/// <summary>
/// Concrete implementation of IItemRepository using Entity Framework Core and PostGIS.
/// Handles spatial queries for location-based item discovery.
/// </summary>
public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;
    private readonly GeometryFactory _geometryFactory;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
        // SRID 4326 is standard for GPS (WGS84)
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Item>> GetAllAsync()
    {
        return await _context.Items
            .Include(i => i.Owner)
            .Where(i => i.IsAvailable)
            .ToListAsync();
    }

    /// <summary>
    /// Performs a spatial query using PostGIS ST_DWithin logic.
    /// Finds items within a specified radius of a point.
    /// </summary>
    public async Task<List<Item>> GetNearbyAsync(double lat, double lon, double radiusKm)
    {
        var userPoint = _geometryFactory.CreatePoint(new Coordinate(lon, lat));
        var radiusMeters = radiusKm * 1000;

        return await _context.Items
            .Include(i => i.Owner)
            .Where(i => i.IsAvailable && i.Location != null)
            .Where(i => i.Location.IsWithinDistance(userPoint, radiusMeters))
            .OrderBy(i => i.Location.Distance(userPoint))
            .ToListAsync();
    }

    public async Task<List<Item>> GetByOwnerIdAsync(int ownerId)
    {
        return await _context.Items
            .Where(i => i.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task AddAsync(Item item)
    {
        await _context.Items.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Item item)
    {
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}