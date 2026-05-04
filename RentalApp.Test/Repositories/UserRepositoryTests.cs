using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;

namespace RentalApp.Test.Repositories;

/// <summary>
/// Contains unit tests for the UserRepository to ensure proper data access orchestration.
/// </summary>
public class UserRepositoryTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new AppDbContext(options);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var context = GetDatabaseContext();
        var testEmail = "test@example.com";
        context.Users.Add(new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = testEmail,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        });
        await context.SaveChangesAsync();

        // This will not compile yet - that's expected in TDD
        IUserRepository repository = new UserRepository(context);

        // Act
        var result = await repository.GetByEmailAsync(testEmail);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testEmail, result.Email);
    }
}