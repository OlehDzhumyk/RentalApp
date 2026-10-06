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

    [Fact]
    public async Task LoginAsync_ShouldIgnoreEmailCaseAndSurroundingSpaces()
    {
        var user = new User { Email = "sam@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
        _userRepoMock.Setup(r => r.GetByEmailAsync("sam@example.com")).ReturnsAsync(user);

        var result = await _service.LoginAsync("  Sam@Example.COM ", "password123");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsWrong()
    {
        var user = new User { Email = "sam@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
        _userRepoMock.Setup(r => r.GetByEmailAsync("sam@example.com")).ReturnsAsync(user);

        var result = await _service.LoginAsync("sam@example.com", "wrong-password");

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.False(_service.IsAuthenticated);
    }

    [Fact]
    public async Task LoginAsync_ShouldLoadActiveRolesOnly()
    {
        var user = new User
        {
            Email = "sam@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            UserRoles =
            {
                new UserRole { Role = new Role { Name = RoleConstants.User } },
                new UserRole { Role = new Role { Name = RoleConstants.Admin }, IsActive = false },
            }
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("sam@example.com")).ReturnsAsync(user);

        await _service.LoginAsync("sam@example.com", "password123");

        Assert.True(_service.HasRole("user"));
        Assert.False(_service.HasRole(RoleConstants.Admin));
    }

    [Fact]
    public async Task RegisterAsync_ShouldStoreNormalisedEmail_AndHashedPassword_AndAssignDefaultRole()
    {
        _roleRepoMock.Setup(r => r.GetDefaultRoleAsync()).ReturnsAsync(new Role { Id = 2, Name = RoleConstants.User });
        User? saved = null;
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => { u.Id = 7; saved = u; })
            .Returns(Task.CompletedTask);

        var result = await _service.RegisterAsync(" Sam ", "Taylor", " Sam@Example.com", "password123");

        Assert.True(result.IsSuccess);
        Assert.NotNull(saved);
        Assert.Equal("sam@example.com", saved!.Email);
        Assert.Equal("Sam", saved.FirstName);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", saved.PasswordHash));
        _userRepoMock.Verify(r => r.ExistsAsync("sam@example.com"), Times.Once);
        _userRepoMock.Verify(r => r.AddRoleToUserAsync(7, 2), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearUser_AndRaiseEvent()
    {
        var user = new User { Email = "sam@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
        _userRepoMock.Setup(r => r.GetByEmailAsync("sam@example.com")).ReturnsAsync(user);
        await _service.LoginAsync("sam@example.com", "password123");
        bool? lastState = null;
        _service.AuthenticationStateChanged += (_, isAuthenticated) => lastState = isAuthenticated;

        await _service.LogoutAsync();

        Assert.False(_service.IsAuthenticated);
        Assert.Null(_service.CurrentUser);
        Assert.False(lastState);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldRequireCurrentPassword()
    {
        var user = new User { Email = "sam@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
        _userRepoMock.Setup(r => r.GetByEmailAsync("sam@example.com")).ReturnsAsync(user);
        await _service.LoginAsync("sam@example.com", "password123");

        Assert.False(await _service.ChangePasswordAsync("wrong-password", "new-password"));
        Assert.True(await _service.ChangePasswordAsync("password123", "new-password"));

        Assert.True(BCrypt.Net.BCrypt.Verify("new-password", user.PasswordHash));
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}
