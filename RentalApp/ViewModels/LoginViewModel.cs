/*
 * @file LoginViewModel.cs
 * @brief Logic for user authentication and navigation handling
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// Manages the authentication flow, including input validation and secure navigation.
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    /// <summary>
    /// Initializes the ViewModel with required services via Dependency Injection.
    /// </summary>
    /// <param name="authService">Service handling authentication logic.</param>
    /// <param name="navigationService">Service handling platform-agnostic navigation.</param>
    public LoginViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Login";
    }

    /// <summary>
    /// Validates credentials and navigates to the main application hub upon success.
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter both email and password");
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _authService.LoginAsync(Email, Password);

            if (result.IsSuccess)
            {
                await _navigationService.NavigateToAsync("//MainPage");
            }
            else
            {
                SetError(result.Message);
            }
        }
        catch (Exception ex)
        {
            SetError($"Authentication critical failure: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Redirects the user to the account creation interface.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await _navigationService.NavigateToAsync("RegisterPage");
    }

    /// <summary>
    /// Initiates the password recovery workflow.
    /// </summary>
    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Info", "Password recovery is currently under maintenance.", "OK");
        }
    }
}
