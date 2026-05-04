/*
 * @file AuthenticationService.cs
 * @brief Service responsible for user authentication and session management
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using BCryptNet = BCrypt.Net.BCrypt;

namespace RentalApp.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private User? _currentUser;
    private List<string> _currentUserRoles = new();

    public event EventHandler<bool>? AuthenticationStateChanged;

    public AuthenticationService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public bool IsAuthenticated => _currentUser != null;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => _currentUserRoles;

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);

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

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            if (await _userRepository.ExistsAsync(email))
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

            await _userRepository.AddAsync(user);

            // Assign default role using repositories
            var defaultRole = await _roleRepository.GetDefaultRoleAsync();
            if (defaultRole != null)
            {
                await _userRepository.AddRoleToUserAsync(user.Id, defaultRole.Id);
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

            await _userRepository.UpdateAsync(_currentUser);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
