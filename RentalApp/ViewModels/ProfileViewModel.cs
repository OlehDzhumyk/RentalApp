/*
 * @file ProfileViewModel.cs
 * @brief User profile management view model
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// View model for the user profile page.
/// Manages user profile display and password change functionality.
/// </summary>
public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    /// <summary>The current user's profile information.</summary>
    [ObservableProperty]
    public partial User? CurrentUser { get; set; }

    /// <summary>The user's current password for verification.</summary>
    [ObservableProperty]
    public partial string CurrentPassword { get; set; } = string.Empty;

    /// <summary>The user's new password.</summary>
    [ObservableProperty]
    public partial string NewPassword { get; set; } = string.Empty;

    /// <summary>Confirmation of the user's new password.</summary>
    [ObservableProperty]
    public partial string ConfirmNewPassword { get; set; } = string.Empty;

    /// <summary>Indicates whether the password change mode is active.</summary>
    [ObservableProperty]
    public partial bool IsChangingPassword { get; set; }

    /// <summary>
    /// Default constructor for design-time support.
    /// </summary>
    public ProfileViewModel()
    {
        Title = "Profile";
        _authService = null!;
        _navigationService = null!;
    }

    /// <summary>
    /// Initializes a new instance of the ProfileViewModel class.
    /// </summary>
    /// <param name="authService">The authentication service instance.</param>
    /// <param name="navigationService">The navigation service instance.</param>
    public ProfileViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Profile";

        LoadUserData();
    }

    /// <summary>
    /// Retrieves the current user's information from the authentication service.
    /// </summary>
    private void LoadUserData()
    {
        if (_authService != null)
        {
            CurrentUser = _authService.CurrentUser;
        }
    }

    /// <summary>
    /// Validates and performs the password change operation.
    /// </summary>
    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (IsBusy)
            return;

        if (!ValidatePasswordChange())
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);

            if (success)
            {
                if (Shell.Current != null)
                {
                    // FIXED: Use DisplayAlertAsync instead of DisplayAlert
                    await Shell.Current.DisplayAlertAsync("Success", "Password changed successfully!", "OK");
                }
                ClearPasswordFields();
                IsChangingPassword = false;
            }
            else
            {
                SetError("Failed to change password. Please check your current password.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Password change failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Toggles the visibility of password change fields.
    /// </summary>
    [RelayCommand]
    private void TogglePasswordChangeMode()
    {
        IsChangingPassword = !IsChangingPassword;
        if (!IsChangingPassword)
        {
            ClearPasswordFields();
            ClearError();
        }
    }

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        if (_navigationService != null)
        {
            await _navigationService.NavigateBackAsync();
        }
    }

    /// <summary>
    /// Validates password change requirements.
    /// </summary>
    /// <returns>True if validation passes, false otherwise.</returns>
    private bool ValidatePasswordChange()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            SetError("Current password is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            SetError("New password is required");
            return false;
        }

        if (NewPassword.Length < 6)
        {
            SetError("New password must be at least 6 characters long");
            return false;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            SetError("New passwords do not match");
            return false;
        }

        if (CurrentPassword == NewPassword)
        {
            SetError("New password must be different from current password");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Resets all password input fields to empty strings.
    /// </summary>
    private void ClearPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
    }
}