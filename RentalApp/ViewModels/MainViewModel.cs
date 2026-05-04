/*
 * @file MainViewModel.cs
 * @brief Main dashboard view model for authenticated users
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// View model for the main dashboard page.
/// Manages the main dashboard display, user information, and navigation to other sections.
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    /// <summary>The currently authenticated user information.</summary>
    [ObservableProperty]
    public partial User? CurrentUser { get; set; }

    /// <summary>Personalized welcome message displayed to the user.</summary>
    [ObservableProperty]
    public partial string WelcomeMessage { get; set; } = string.Empty;

    /// <summary>Indicates whether the current user has admin privileges.</summary>
    [ObservableProperty]
    public partial bool IsAdmin { get; set; }

    /// <summary>
    /// Default constructor for design-time support.
    /// </summary>
    public MainViewModel()
    {
        Title = "Dashboard";
        _authService = null!;
        _navigationService = null!;
    }

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    public MainViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Dashboard";

        LoadUserData();
    }

    private void LoadUserData()
    {
        if (_authService == null) return;

        CurrentUser = _authService.CurrentUser;
        IsAdmin = _authService.HasRole("Admin");

        if (CurrentUser != null)
        {
            WelcomeMessage = $"Welcome, {CurrentUser.FullName}!";
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (Shell.Current == null) return;

        bool result = await Shell.Current.DisplayAlertAsync(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (result)
        {
            await _authService.LogoutAsync();
            await _navigationService.NavigateToAsync("LoginPage");
        }
    }

    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await _navigationService.NavigateToAsync("TempPage");
    }

    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await _navigationService.NavigateToAsync("TempPage");
    }

    [RelayCommand]
    private async Task NavigateToUserListAsync()
    {
        if (!IsAdmin)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Access Denied", "You don't have permission to access admin features.", "OK");
            }
            return;
        }

        await _navigationService.NavigateToAsync("UserListPage");
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        try
        {
            IsBusy = true;
            LoadUserData();
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            SetError($"Failed to refresh data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}