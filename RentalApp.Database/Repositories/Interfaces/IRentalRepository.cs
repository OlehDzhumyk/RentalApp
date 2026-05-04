/*
 * @file IRentalRepository.cs
 * @brief Interface for managing rental data access
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

public interface IRentalRepository
{
    /// <summary>
    /// Retrieves all rentals for a specific item.
    /// </summary>
    Task<List<Rental>> GetByItemIdAsync(int itemId);

    /// <summary>
    /// Finds a specific rental by its unique identifier.
    /// </summary>
    Task<Rental?> GetByIdAsync(int id);

    /// <summary>
    /// Persists a new rental record.
    /// </summary>
    Task<Rental> CreateAsync(Rental rental);

    /// <summary>
    /// Updates an existing rental record.
    /// </summary>
    Task UpdateAsync(Rental rental);
}
