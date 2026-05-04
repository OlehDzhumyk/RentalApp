/*
 * @file AuthenticationServiceTests.cs
 * @brief Unit tests for AuthenticationService using Moq to isolate business logic
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;

namespace RentalApp.Test.Unit.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _service = new AuthenticationService(_userRepoMock.Object, _roleRepoMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFail_WhenUserExists()
    {
        // Arrange
        var email = "exists@test.com";
        _userRepoMock.Setup(r => r.ExistsAsync(email)).ReturnsAsync(true);

        // Act
        var result = await _service.RegisterAsync("Test", "User", email, "password123");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User with this email already exists", result.Message);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "valid@test.com";
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User { Email = email, PasswordHash = hashedPassword };
        _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);

        // Act
        var result = await _service.LoginAsync(email, password);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(_service.IsAuthenticated);
        Assert.Equal(user, _service.CurrentUser);
    }
}
