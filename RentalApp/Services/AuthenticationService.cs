using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using BCryptNet = BCrypt.Net.BCrypt;

namespace RentalApp.Services
{

    /// <summary>
    /// Service responsible for user authentication, registration, and session management.
    /// Now uses IUserRepository for data access to follow the Repository Pattern.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context; // Keep context for roles/UserRoles until they get repositories
        private User? _currentUser;
        private List<string> _currentUserRoles = new();

        public event EventHandler<bool>? AuthenticationStateChanged;

        public AuthenticationService(IUserRepository userRepository, AppDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        public bool IsAuthenticated => _currentUser != null;
        public User? CurrentUser => _currentUser;
        public List<string> CurrentUserRoles => _currentUserRoles;

        /// <summary>
        /// Authenticates a user with email and password using the repository.
        /// </summary>
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

        /// <summary>
        /// Registers a new user using the repository for existence checks and persistence.
        /// </summary>
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
        /// Updates user password via the repository.
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

                await _userRepository.UpdateAsync(_currentUser);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}