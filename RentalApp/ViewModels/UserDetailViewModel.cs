/*
 * @file UserDetailViewModel.cs
 * @brief ViewModel for managing user details using the Repository Pattern
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for managing user details including creation, editing, and role management.
/// Orchestrates data via IUserRepository and IRoleRepository to maintain clean architecture.
/// </summary>
[QueryProperty(nameof(UserId), "userId")]
public partial class UserDetailViewModel : BaseViewModel
{
    #region Private Fields

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authService;

    private User? _currentUser;

    #endregion

    #region Observable Properties

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

    public UserDetailViewModel(
        IUserRepository userRepository, 
        IRoleRepository roleRepository,
        INavigationService navigationService, 
        IAuthenticationService authService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        Title = "User Details";
    }

    #endregion

    #region Computed Properties & State Handlers

    public string PageTitle => IsNewUser ? "Create New User" : "Edit User";
    public bool ShowPasswordFields => IsNewUser;
    public bool CanDeleteCurrentUser => !IsNewUser && _currentUser?.Id != _authService.CurrentUser?.Id;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsBusy))
        {
            SaveUserCommand.NotifyCanExecuteChanged();
            DeleteUserCommand.NotifyCanExecuteChanged();
            NavigateBackCommand.NotifyCanExecuteChanged();
        }
        
        // Load user data when UserId is set via query property
        if (e.PropertyName == nameof(UserId))
        {
            _ = LoadUserAsync();
        }
    }

    #endregion

    #region Commands

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

    [RelayCommand(CanExecute = nameof(CanDeleteUser))]
    private async Task DeleteUserAsync()
    {
        if (_currentUser == null) return;

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
            await _userRepository.UpdateAsync(_currentUser);

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

    [RelayCommand]
    private async Task AddRoleAsync(RoleItem role)
    {
        if (role == null || _currentUser == null || role.IsAssigned) return;

        try
        {
            await _userRepository.AddRoleToUserAsync(_currentUser.Id, role.Id);
            role.IsAssigned = true;
            SuccessMessage = $"Role '{role.Name}' added.";
        }
        catch (Exception ex)
        {
            SetError($"Error adding role: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RemoveRoleAsync(RoleItem role)
    {
        if (role == null || _currentUser == null || !role.IsAssigned) return;

        try
        {
            await _userRepository.RemoveRoleFromUserAsync(_currentUser.Id, role.Id);
            role.IsAssigned = false;
            SuccessMessage = $"Role '{role.Name}' removed.";
        }
        catch (Exception ex)
        {
            SetError($"Error removing role: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await _navigationService.NavigateToAsync("UserListPage");
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    #endregion

    #region Private Methods

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
            var allRoles = await _roleRepository.GetAllAsync();

            if (UserId == 0)
            {
                InitializeNewUser(allRoles);
            }
            else
            {
                await InitializeExistingUserAsync(allRoles);
            }

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

    private async Task InitializeExistingUserAsync(List<Role> allRoles)
    {
        IsNewUser = false;
        _currentUser = await _userRepository.GetByIdAsync(UserId);

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

    private async Task CreateUserInternalAsync()
    {
        if (await _userRepository.ExistsAsync(Email.Trim()))
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

        await _userRepository.AddAsync(user);

        // Map initial roles if any were selected during creation
        var selectedRoles = AvailableRoles.Where(r => r.IsAssigned).ToList();
        foreach (var role in selectedRoles)
        {
            await _userRepository.AddRoleToUserAsync(user.Id, role.Id);
        }

        _currentUser = user;
        IsNewUser = false;
    }

    private async Task UpdateUserInternalAsync()
    {
        if (_currentUser == null) return;

        var emailTrimmed = Email.Trim();
        var existingUser = await _userRepository.GetByEmailAsync(emailTrimmed);

        if (existingUser != null && existingUser.Id != _currentUser.Id)
        {
            throw new InvalidOperationException("Email is already used by another user.");
        }

        _currentUser.FirstName = FirstName.Trim();
        _currentUser.LastName = LastName.Trim();
        _currentUser.Email = emailTrimmed;
        _currentUser.IsActive = IsActive;
        _currentUser.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(_currentUser);
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            SetError("First and last names are required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email.Trim()))
        {
            SetError("Please enter a valid email address.");
            return false;
        }

        if (IsNewUser)
        {
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                SetError("Password is required and must be at least 6 characters.");
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

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }

    private void ClearMessages()
    {
        ClearError();
        SuccessMessage = string.Empty;
    }

    #endregion
}