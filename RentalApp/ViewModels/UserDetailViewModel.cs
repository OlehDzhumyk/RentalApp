using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for managing user details including creation, editing, and role management.
/// Handles both new user creation and existing user modification scenarios.
/// </summary>
[QueryProperty(nameof(UserId), "userId")]
public partial class UserDetailViewModel : BaseViewModel
{
    #region Private Fields

    /// <summary>Database context for data operations.</summary>
    private readonly AppDbContext _context;

    /// <summary>Navigation service for page transitions.</summary>
    private readonly INavigationService _navigationService;

    /// <summary>Authentication service for user role verification.</summary>
    private readonly IAuthenticationService _authService;

    /// <summary>The current user entity being edited.</summary>
    private User? _currentUser;

    #endregion

    #region Observable Properties

    /// <summary>The ID of the user being edited.</summary>
    [ObservableProperty]
    public partial int UserId { get; set; }

    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsActive { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(ShowPasswordFields))]
    [NotifyPropertyChangedFor(nameof(CanDeleteCurrentUser))]
    public partial bool IsNewUser { get; set; }

    [ObservableProperty]
    public partial string SuccessMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<RoleItem> AvailableRoles { get; set; } = new();

    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the UserDetailViewModel class.
    /// </summary>
    public UserDetailViewModel(AppDbContext context, INavigationService navigationService, IAuthenticationService authService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        Title = "User Details";
    }

    #endregion

    #region Computed Properties & State Handlers

    /// <summary>Gets the page title based on the current user state.</summary>
    public string PageTitle => IsNewUser ? "Create New User" : "Edit User";

    /// <summary>Visibility logic for password fields.</summary>
    public bool ShowPasswordFields => IsNewUser;

    /// <summary>Prevents users from deleting their own logged-in account.</summary>
    public bool CanDeleteCurrentUser => !IsNewUser && _currentUser?.Id != _authService.CurrentUser?.Id;

    /// <summary>
    /// Overrides property change notifications to update command states.
    /// Listens for IsBusy changes to refresh button enabled/disabled states.
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsBusy))
        {
            // Генератор автоматично створить ці властивості з суфіксом "Command"
            SaveUserCommand.NotifyCanExecuteChanged();
            DeleteUserCommand.NotifyCanExecuteChanged();
            NavigateBackCommand.NotifyCanExecuteChanged();
        }
    }

    #endregion


    #region Commands

    /// <summary>
    /// Saves the user data (creates new or updates existing).
    /// Executable only when the VM is not busy.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveUser))]
    private async Task SaveUserAsync()
    {
        ClearMessages();
        if (!ValidateInput()) return;

        IsBusy = true;
        try
        {
            if (IsNewUser) await CreateUserInternalAsync();
            else await UpdateUserInternalAsync();

            SuccessMessage = "User saved successfully!";
            if (IsNewUser)
            {
                await Task.Delay(1500);
                await NavigateBackAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Save error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSaveUser() => !IsBusy;

    /// <summary>
    /// Performs a soft delete of the user after confirmation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteUser))]
    private async Task DeleteUserAsync()
    {
        if (_currentUser == null || Shell.Current == null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirm Delete",
            $"Are you sure you want to delete user '{_currentUser.FullName}'?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            _currentUser.IsActive = false;
            _currentUser.DeletedAt = DateTime.UtcNow;
            _context.Users.Update(_currentUser);
            await _context.SaveChangesAsync();

            await NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Delete error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDeleteUser() => !IsBusy && CanDeleteCurrentUser;

    /// <summary>
    /// Assigns a new role to the user and persists changes to the database.
    /// </summary>
    [RelayCommand]
    private async Task AddRoleAsync(RoleItem role)
    {
        if (role == null || _currentUser == null || role.IsAssigned) return;

        try
        {
            var userRole = new UserRole(_currentUser.Id, role.Id);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            role.IsAssigned = true;
            SuccessMessage = $"Role '{role.Name}' added.";
        }
        catch (Exception ex)
        {
            SetError($"Error adding role: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a role from the user (soft delete) and updates the database.
    /// </summary>
    [RelayCommand]
    private async Task RemoveRoleAsync(RoleItem role)
    {
        if (role == null || _currentUser == null || !role.IsAssigned) return;

        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == _currentUser.Id && ur.RoleId == role.Id && ur.IsActive);

            if (userRole != null)
            {
                userRole.MarkAsDeleted();
                _context.UserRoles.Update(userRole);
                await _context.SaveChangesAsync();

                role.IsAssigned = false;
                SuccessMessage = $"Role '{role.Name}' removed.";
            }
        }
        catch (Exception ex)
        {
            SetError($"Error removing role: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates back to the user list management page.
    /// </summary>
    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        if (_navigationService != null)
        {
            await _navigationService.NavigateToAsync("UserListPage");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Navigates to the main dashboard page.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    /// <summary>
    /// Loads user data from the database based on the current UserId.
    /// Initializes new user mode if UserId is 0.
    /// </summary>
    private async Task LoadUserAsync()
    {
        if (!_authService.HasRole(RoleConstants.Admin))
        {
            await _navigationService.NavigateToAsync("MainPage");
            return;
        }

        IsBusy = true;
        try
        {
            var allRoles = await _context.Roles.ToListAsync();

            if (UserId == 0)
            {
                InitializeNewUser(allRoles);
            }
            else
            {
                await InitializeExistingUserAsync(allRoles);
            }

            // Notify UI about state-dependent property changes
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(ShowPasswordFields));
            OnPropertyChanged(nameof(CanDeleteCurrentUser));
        }
        catch (Exception ex)
        {
            SetError($"Error loading user: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Sets up the ViewModel state for creating a new user.
    /// </summary>
    private void InitializeNewUser(List<Role> allRoles)
    {
        IsNewUser = true;
        _currentUser = null;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        IsActive = true;

        AvailableRoles = new ObservableCollection<RoleItem>(
            allRoles.Select(r => new RoleItem
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsAssigned = false
            }));
    }

    /// <summary>
    /// Loads existing user data and maps their assigned roles.
    /// </summary>
    private async Task InitializeExistingUserAsync(List<Role> allRoles)
    {
        IsNewUser = false;
        _currentUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == UserId);

        if (_currentUser == null)
        {
            SetError("User not found.");
            return;
        }

        FirstName = _currentUser.FirstName;
        LastName = _currentUser.LastName;
        Email = _currentUser.Email;
        IsActive = _currentUser.IsActive;

        var userRoleIds = _currentUser.UserRoles
            .Where(ur => ur.IsActive)
            .Select(ur => ur.RoleId)
            .ToList();

        AvailableRoles = new ObservableCollection<RoleItem>(
            allRoles.Select(r => new RoleItem
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsAssigned = userRoleIds.Contains(r.Id)
            }));
    }

    /// <summary>
    /// Internal logic to create a new user record with hashed password and roles.
    /// </summary>
    private async Task CreateUserInternalAsync()
    {
        var existingUser = await _context.Users.AnyAsync(u => u.Email == Email.Trim());
        if (existingUser)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password, salt);

        var user = new User
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Email = Email.Trim(),
            PasswordHash = hashedPassword,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = IsActive
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var selectedRoles = AvailableRoles.Where(r => r.IsAssigned).ToList();
        foreach (var role in selectedRoles)
        {
            _context.UserRoles.Add(new UserRole(user.Id, role.Id));
        }

        if (selectedRoles.Any())
        {
            await _context.SaveChangesAsync();
        }

        _currentUser = user;
        IsNewUser = false;
    }


    /// <summary>
    /// Internal logic to update an existing user's information.
    /// Validates email uniqueness before committing changes.
    /// </summary>
    private async Task UpdateUserInternalAsync()
    {
        if (_currentUser == null) return;

        var emailTrimmed = Email.Trim();

        // Check for email uniqueness among other users
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == emailTrimmed && u.Id != _currentUser.Id);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already used by another user.");
        }

        _currentUser.FirstName = FirstName.Trim();
        _currentUser.LastName = LastName.Trim();
        _currentUser.Email = emailTrimmed;
        _currentUser.IsActive = IsActive;
        _currentUser.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(_currentUser);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Validates all user input fields based on whether a new user is being created or updated.
    /// Sets the UI error message if any validation rule fails.
    /// </summary>
    /// <returns>True if validation passes; otherwise, false.</returns>
    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            SetError("First and last names are required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            SetError("Email is required.");
            return false;
        }

        if (!IsValidEmail(Email.Trim()))
        {
            SetError("Please enter a valid email address.");
            return false;
        }

        if (IsNewUser)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                SetError("Password is required.");
                return false;
            }

            if (Password.Length < 6)
            {
                SetError("Password must be at least 6 characters long.");
                return false;
            }

            if (Password != ConfirmPassword)
            {
                SetError("Passwords do not match.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates the format of an email address.
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException) { return false; }
    }

    /// <summary>
    /// Resets error and success messages to their default empty states.
    /// </summary>
    private void ClearMessages()
    {
        ClearError(); // Method from BaseViewModel
        SuccessMessage = string.Empty;
    }

    #endregion
} // End of UserDetailViewModel