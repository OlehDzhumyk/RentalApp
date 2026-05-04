using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllActiveAsync();
    Task<List<User>> GetAllWithRolesAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
    Task AddRoleToUserAsync(int userId, int roleId);
    Task RemoveRoleFromUserAsync(int userId, int roleId);
}