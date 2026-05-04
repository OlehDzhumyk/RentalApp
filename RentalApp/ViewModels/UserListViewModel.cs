/*
 * @file UserListViewModel.cs
 * @brief ViewModel for managing the user list display and interactions.
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for managing the user list display and interactions in the application.
/// Provides functionality for loading, filtering, searching, and navigating users.
/// Requires admin privileges to function properly.
/// </summary>
public partial class UserListViewModel : BaseViewModel
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    private ObservableCollection<UserListItem> _users = new();
    private ObservableCollection<UserListItem> _filteredUsers = new();
    private string _selectedRoleFilter = "All";
    private string _searchText = string.Empty;
    private bool _isLoading = false;
    private bool _isRefreshing = false;

    /// <summary>
    /// Initializes a new instance of the UserListViewModel class.
    /// </summary>
    public UserListViewModel(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        INavigationService navigationService,
        IAuthenticationService authenticationService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

        LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshUsersAsync);
        UserSelectedCommand = new AsyncRelayCommand<UserListItem?>(NavigateToUserDetailAsync);
        CreateUserCommand = new AsyncRelayCommand(NavigateToCreateUserAsync);

        RoleFilterOptions = new ObservableCollection<string> { "All" };

        InitializeViewModel();
    }

    private async void InitializeViewModel()
    {
        await LoadRoleFiltersAsync();
        await LoadUsersAsync();
    }

    private async Task LoadRoleFiltersAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        foreach (var role in roles)
        {
            if (!RoleFilterOptions.Contains(role.Name))
                RoleFilterOptions.Add(role.Name);
        }
    }

    public ObservableCollection<UserListItem> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    public ObservableCollection<UserListItem> FilteredUsers
    {
        get => _filteredUsers;
        set => SetProperty(ref _filteredUsers, value);
    }

    public ObservableCollection<string> RoleFilterOptions { get; }

    public string SelectedRoleFilter
    {
        get => _selectedRoleFilter;
        set
        {
            if (SetProperty(ref _selectedRoleFilter, value))
                ApplyFilters();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilters();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public bool IsAdmin => _authenticationService.HasRole(RoleConstants.Admin);

    public ICommand LoadUsersCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand UserSelectedCommand { get; }
    public ICommand CreateUserCommand { get; }

    private async Task LoadUsersAsync()
    {
        if (!IsAdmin)
        {
            await _navigationService.NavigateToAsync("//MainPage");
            return;
        }

        IsLoading = true;
        try
        {
            var users = await _userRepository.GetAllWithRolesAsync();

            var userItems = users.Select(u => new UserListItem
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt ?? DateTime.MinValue,
                IsActive = u.IsActive,
                Roles = u.UserRoles
                    .Where(ur => ur.IsActive)
                    .Select(ur => ur.Role.Name)
                    .ToList(),
                RolesDisplay = string.Join(", ", u.UserRoles
                    .Where(ur => ur.IsActive)
                    .Select(ur => ur.Role.Name))
            }).ToList();

            Users = new ObservableCollection<UserListItem>(userItems);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            SetError($"Error loading users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshUsersAsync()
    {
        IsRefreshing = true;
        await LoadUsersAsync();
        IsRefreshing = false;
    }

    private void ApplyFilters()
    {
        var filtered = Users.AsEnumerable();

        if (SelectedRoleFilter != "All")
        {
            filtered = filtered.Where(u => u.Roles.Contains(SelectedRoleFilter));
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(u =>
                u.FullName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                u.RolesDisplay.ToLower().Contains(searchLower));
        }

        FilteredUsers = new ObservableCollection<UserListItem>(filtered);
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    private async Task NavigateToUserDetailAsync(UserListItem? user)
    {
        if (user != null)
        {
            await _navigationService.NavigateToAsync($"UserDetailPage?userId={user.Id}");
        }
    }

    private async Task NavigateToCreateUserAsync()
    {
        await _navigationService.NavigateToAsync("UserDetailPage?userId=0");
    }
}

public class UserListItem
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public string RolesDisplay { get; set; } = string.Empty;
}