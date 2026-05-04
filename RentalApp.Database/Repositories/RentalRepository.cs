/*
 * @file RentalRepository.cs
 * @brief Repository implementation for Rental entity operations
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

/// <summary>
/// Handles data access logic for Rental entities using Entity Framework Core.
/// </summary>
public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all rentals associated with a specific item to check for date overlaps.
    /// </summary>
    public async Task<List<Rental>> GetByItemIdAsync(int itemId)
    {
        return await _context.Rentals
            .Where(r => r.ItemId == itemId)
            .ToListAsync();
    }

    /// <summary>
    /// Persists a new rental record to the database.
    /// </summary>
    public async Task<Rental> CreateAsync(Rental rental)
    {
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();
        return rental;
    }
}
