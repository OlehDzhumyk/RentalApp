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
    /// Finds available items within a radius of a point, nearest first.
    /// Location is a geography column, so PostGIS works in metres: IsWithinDistance becomes
    /// ST_DWithin (which can use the GIST index) and Distance becomes ST_Distance.
    /// </summary>
    public async Task<List<NearbyItem>> GetNearbyAsync(double lat, double lon, double radiusKm)
    {
        var userPoint = _geometryFactory.CreatePoint(new Coordinate(lon, lat));
        var radiusMeters = radiusKm * 1000;

        var results = await _context.Items
            .Include(i => i.Owner)
            .Where(i => i.IsAvailable && i.Location != null)
            .Where(i => i.Location!.IsWithinDistance(userPoint, radiusMeters))
            .Select(i => new { Item = i, DistanceMeters = i.Location!.Distance(userPoint) })
            .OrderBy(r => r.DistanceMeters)
            .ToListAsync();

        return results.Select(r => new NearbyItem(r.Item, r.DistanceMeters / 1000)).ToList();
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
