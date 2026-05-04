/*
 * @file IRentalRepository.cs
 * @brief Interface for managing rental data access
 */
using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

public interface IRentalRepository
{
    Task<List<Rental>> GetByItemIdAsync(int itemId);
    Task<Rental> CreateAsync(Rental rental);
}
