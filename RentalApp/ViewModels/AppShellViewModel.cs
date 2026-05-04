/*
 * @file AppShellViewModel.cs
 * @brief Application shell view model for managing navigation and authentication state
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RentalApp.ViewModels;

/// <summary>
/// View model for the application shell that manages navigation and authentication.
/// Handles menu items, navigation commands, and authentication state changes.
/// </summary>
public partial class AppShellViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService = default!;
    private readonly INavigationService _navigationService = default!;

    /// <summary>
    /// Collection of dynamic menu bar items that can be modified at runtime based on permissions.
    /// </summary>
    public ObservableCollection<MenuBarItem> DynamicMenuBarItems { get; } = new();

    /// <summary>
    /// Default constructor for design-time support.
    /// </summary>
    public AppShellViewModel()
    {
        Title = "RentalApp";
    }

    /// <summary>
    /// Initializes a new instance of the AppShellViewModel class.
    /// </summary>
    public AppShellViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        Title = "RentalApp";
    }

    private bool CanExecuteAuthenticatedAction() => _authService?.IsAuthenticated ?? false;

    private void OnAuthenticationStateChanged(object? sender, bool isAuthenticated)
    {
        LogoutCommand.NotifyCanExecuteChanged();
        NavigateToProfileCommand.NotifyCanExecuteChanged();
        NavigateToSettingsCommand.NotifyCanExecuteChanged();

        Debug.WriteLine($"Authentication state changed: {isAuthenticated}");
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

    [RelayCommand(CanExecute = nameof(CanExecuteAuthenticatedAction))]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        await _navigationService.NavigateToAsync("LoginPage");
    }
}