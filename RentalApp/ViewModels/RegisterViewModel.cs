/*
 * @file RegisterViewModel.cs
 * @brief User registration view model
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Services;
using System.Text.RegularExpressions;

namespace RentalApp.ViewModels;

/// <summary>
/// View model for the user registration page.
/// Manages user registration form data, validation, and registration process.
/// </summary>
public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    /// <summary>The user's first name bound to the input field.</summary>
    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;

    /// <summary>The user's last name bound to the input field.</summary>
    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;

    /// <summary>The user's email address bound to the input field.</summary>
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    /// <summary>The user's password bound to the input field.</summary>
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    /// <summary>Confirmation of the user's password.</summary>
    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Whether the user accepts the terms and conditions.</summary>
    [ObservableProperty]
    public partial bool AcceptTerms { get; set; }

    /// <summary>
    /// Default constructor for design-time support.
    /// Sets the title to "Register".
    /// </summary>
    public RegisterViewModel()
    {
        Title = "Register";
        _authService = null!;
        _navigationService = null!;
    }

    /// <summary>
    /// Initializes a new instance of the RegisterViewModel class.
    /// </summary>
    /// <param name="authService">The authentication service instance.</param>
    /// <param name="navigationService">The navigation service instance.</param>
    public RegisterViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Register";
    }

    /// <summary>
    /// Registers a new user account after validating the form data.
    /// </summary>
    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy)
            return;

        if (!ValidateForm())
            return;

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _authService.RegisterAsync(FirstName, LastName, Email, Password);

            if (result.IsSuccess)
            {
                if (Shell.Current != null)
                {
                    // FIXED: Use DisplayAlertAsync
                    await Shell.Current.DisplayAlertAsync("Success", "Registration successful! Please login.", "OK");
                }
                await _navigationService.NavigateBackAsync();
            }
            else
            {
                SetError(result.Message);
            }
        }
        catch (Exception ex)
        {
            SetError($"Registration failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Returns to the login page.
    /// </summary>
    [RelayCommand]
    private async Task NavigateBackToLoginAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    /// <summary>
    /// Validates all registration requirements and sets appropriate error messages.
    /// </summary>
    /// <returns>True if validation passes, false otherwise.</returns>
    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            SetError("First name is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            SetError("Last name is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            SetError("Email is required");
            return false;
        }

        if (!IsValidEmail(Email))
        {
            SetError("Please enter a valid email address");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            SetError("Password is required");
            return false;
        }

        if (Password.Length < 6)
        {
            SetError("Password must be at least 6 characters long");
            return false;
        }

        if (Password != ConfirmPassword)
        {
            SetError("Passwords do not match");
            return false;
        }

        if (!AcceptTerms)
        {
            SetError("Please accept the terms and conditions");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates email address format using regular expression.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
    }
}