/*
 * @file RoleRepositoryTests.cs
 * @brief Integration tests for IRoleRepository ensuring proper role management and seeding
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace RentalApp.Test.Integration.Repositories;

[Collection("Database collection")]
public class RoleRepositoryTests : BaseIntegrationTest
{
    private readonly IRoleRepository _repository;

    public RoleRepositoryTests()
    {
        _repository = ServiceProvider.GetRequiredService<IRoleRepository>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSeededRoles()
    {
        // Act
        var roles = await _repository.GetAllAsync();

        // Assert: Based on AppDbContext seeding (Admin and User roles)
        Assert.NotNull(roles);
        Assert.True(roles.Count >= 2);
        Assert.Contains(roles, r => r.Name == "Admin");
        Assert.Contains(roles, r => r.Name == "User");
    }

    [Fact]
    public async Task GetDefaultRoleAsync_ShouldReturnUserRole()
    {
        // Act
        var defaultRole = await _repository.GetDefaultRoleAsync();

        // Assert: 'User' is marked as IsDefault = true in our migrations
        Assert.NotNull(defaultRole);
        Assert.Equal("User", defaultRole.Name);
        Assert.True(defaultRole.IsDefault);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnCorrectRole()
    {
        // Act
        var adminRole = await _repository.GetByNameAsync("Admin");

        // Assert
        Assert.NotNull(adminRole);
        Assert.Equal("Admin", adminRole.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_ForNonExistentRole()
    {
        // Act
        var result = await _repository.GetByNameAsync("Superuser");

        // Assert
        Assert.Null(result);
    }
}
