/*
 * @file LoginViewModel.cs
 * @brief Login page view model for user authentication
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// View model for the login page that handles user authentication.
/// Manages login form data, validation, and the authentication process.
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    /// <summary>The user's email address bound to the input field.</summary>
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    /// <summary>The user's password bound to the input field.</summary>
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    /// <summary>Whether to remember the user's login credentials.</summary>
    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    /// <summary>
    /// Default constructor for design-time support.
    /// </summary>
    public LoginViewModel()
    {
        Title = "Login";
        _authService = null!;
        _navigationService = null!;
    }

    /// <summary>
    /// Initializes a new instance of the LoginViewModel class.
    /// </summary>
    /// <param name="authService">The authentication service instance.</param>
    /// <param name="navigationService">The navigation service instance.</param>
    public LoginViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Login";
    }

    /// <summary>
    /// Performs user login authentication.
    /// Validates input and attempts to authenticate via the authentication service.
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter both email and password");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _authService.LoginAsync(Email, Password);

            if (result.IsSuccess)
            {
                await _navigationService.NavigateToAsync("MainPage");
            }
            else
            {
                SetError(result.Message);
            }
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigates to the user registration page.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await _navigationService.NavigateToAsync("RegisterPage");
    }

    /// <summary>
    /// Handles forgot password functionality with a placeholder alert.
    /// </summary>
    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Info", "Forgot password functionality not implemented yet", "OK");
        }
    }
}
