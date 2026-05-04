using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

/// <summary>
/// Defines the contract for user data operations to decouple business logic from EF Core.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllActiveAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
}