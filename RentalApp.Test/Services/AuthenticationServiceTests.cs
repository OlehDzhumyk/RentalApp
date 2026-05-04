/*
 * @file AuthenticationServiceTests.cs
 * @brief Unit tests for AuthenticationService business logic
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;

namespace RentalApp.Test.Services;

/// <summary>
/// Unit tests for AuthenticationService using mocked dependencies.
/// Focuses on verifying login and registration workflows without database overhead.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();

        // We inject the mocked repository. 
        // AppDbContext is passed as null! since it's not used in the LoginAsync logic.
        _authService = new AuthenticationService(_userRepositoryMock.Object, null!);
    }

    /// <summary>
    /// Verifies that LoginAsync returns a successful result when repository finds a matching user.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Email = email,
            PasswordHash = hashedPassword,
            IsActive = true
        };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Login successful", result.Message);
    }

    /// <summary>
    /// Verifies that LoginAsync returns failure when the email is not found in the database.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(email, "anyPassword");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password", result.Message);
    }
}