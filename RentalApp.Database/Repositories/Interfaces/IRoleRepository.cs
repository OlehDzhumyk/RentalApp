/*
 * @file IRoleRepository.cs
 * @brief Interface for role data operations
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

/// <summary>
/// Defines the contract for role-related data access.
/// </summary>
public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetDefaultRoleAsync();
    Task<Role?> GetByNameAsync(string name);
}