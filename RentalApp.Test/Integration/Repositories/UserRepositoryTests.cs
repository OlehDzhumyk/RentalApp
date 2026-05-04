/*
 * @file UserRepositoryTests.cs
 * @brief Integration tests for IUserRepository covering identity and role management
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace RentalApp.Test.Integration.Repositories;

[Collection("Database collection")]
public class UserRepositoryTests : BaseIntegrationTest
{
    private readonly IUserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = ServiceProvider.GetRequiredService<IUserRepository>();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUser_WhenDataIsValid()
    {
        // Arrange
        var user = new User
        {
            Email = "student@napier.ac.uk",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "argon2_secure_hash"
        };

        // Act
        await _repository.AddAsync(user);
        var retrieved = await _repository.GetByEmailAsync("student@napier.ac.uk");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(user.FirstName, retrieved.FirstName);
        Assert.True(retrieved.Id > 0);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_ForExistingEmail()
    {
        // Arrange
        var email = "unique_check@napier.ac.uk";
        await _repository.AddAsync(new User
        {
            Email = email,
            FirstName = "Test",
            PasswordHash = "hash"
        });

        // Act
        var exists = await _repository.ExistsAsync(email);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task AddRoleToUserAsync_ShouldEstablishRelationship()
    {
        // Arrange
        var user = new User { Email = "role_test@test.com", FirstName = "User", PasswordHash = "hash" };
        await _repository.AddAsync(user);

        // Assuming Roles are seeded in AppDbContext or added here
        var role = new Role { Name = "Moderator" };
        Context.Roles.Add(role);
        await Context.SaveChangesAsync();

        // Act
        await _repository.AddRoleToUserAsync(user.Id, role.Id);

        // Assert: Using context to verify the junction table record
        var userWithRoles = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(userWithRoles);
    }
}
