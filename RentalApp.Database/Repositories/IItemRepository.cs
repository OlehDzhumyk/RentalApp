/*
 * @file IItemRepository.cs
 * @brief Interface for item data operations
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

/// <summary>
/// Defines the contract for managing rental items in the database.
/// </summary>
public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id);
    Task<List<Item>> GetAllAsync();
    Task<List<Item>> GetByOwnerIdAsync(int ownerId);
    Task AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
}