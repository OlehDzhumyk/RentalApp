using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace RentalApp.Services;

/// <summary>
/// Service responsible for user authentication, registration, and session management.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;
    private User? _currentUser;
    private List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public AuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    public bool IsAuthenticated => _currentUser != null;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || !BCryptNet.Verify(password, user.PasswordHash))
            {
                return new AuthenticationResult(false, "Invalid email or password");
            }

            _currentUser = user;
            _currentUserRoles = user.UserRoles
                .Where(ur => ur.IsActive)
                .Select(ur => ur.Role.Name)
                .ToList();

            AuthenticationStateChanged?.Invoke(this, true);
            return new AuthenticationResult(true, "Login successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers a new user and assigns the default role.
    /// </summary>
    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == email);
            if (existingUser)
            {
                return new AuthenticationResult(false, "User with this email already exists");
            }

            var salt = BCryptNet.GenerateSalt();
            var hashedPassword = BCryptNet.HashPassword(password, salt);

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.IsDefault);
            if (defaultRole != null)
            {
                _context.UserRoles.Add(new UserRole(user.Id, defaultRole.Id));
                await _context.SaveChangesAsync();
            }

            return new AuthenticationResult(true, "Registration successful");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    public Task LogoutAsync()
    {
        _currentUser = null;
        _currentUserRoles.Clear();
        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public bool HasRole(string roleName) =>
        _currentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public bool HasAnyRole(params string[] roleNames) =>
        roleNames.Any(HasRole);

    public bool HasAllRoles(params string[] roleNames) =>
        roleNames.All(HasRole);

    /// <summary>
    /// Validates current password and updates to a new one.
    /// </summary>
    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (_currentUser == null || !BCryptNet.Verify(currentPassword, _currentUser.PasswordHash))
        {
            return false;
        }

        try
        {
            var salt = BCryptNet.GenerateSalt();
            var hashedPassword = BCryptNet.HashPassword(newPassword, salt);

            _currentUser.PasswordHash = hashedPassword;
            _currentUser.PasswordSalt = salt;
            _currentUser.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(_currentUser);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class AuthenticationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }

    public AuthenticationResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
}