using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;


/// <summary>
/// ViewModel for managing the user list display and interactions in the application.
/// Provides functionality for loading, filtering, searching, and navigating users.
/// Implements MVVM pattern with data binding support through INotifyPropertyChanged.
/// </summary>
/// <remarks>
/// This ViewModel requires admin privileges to function properly. Non-admin users
/// will be redirected to the main page when attempting to load users.
/// 
/// Key features:
/// - User loading with role information
/// - Real-time filtering by role and search text
/// - Pull-to-refresh functionality
/// - Navigation to user details and creation
/// - Admin-only access control
/// </remarks>
namespace RentalApp.ViewModels
{
    public partial class UserListViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IUserRepository _userRepository; // Замість DbContext
        private readonly IRoleRepository _roleRepository; // Додано
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;

        private ObservableCollection<UserListItem> _users = new();
        private ObservableCollection<UserListItem> _filteredUsers = new();
        private string _selectedRoleFilter = "All";
        private string _searchText = string.Empty;
        private bool _isLoading = false;
        private bool _isRefreshing = false;

        public UserListViewModel(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            INavigationService navigationService,
            IAuthenticationService authenticationService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshUsersAsync);
            UserSelectedCommand = new AsyncRelayCommand<UserListItem>(NavigateToUserDetailAsync);
            CreateUserCommand = new AsyncRelayCommand(NavigateToCreateUserAsync);

            RoleFilterOptions = new ObservableCollection<string> { "All" };

            // Викликаємо ініціалізацію без блокування конструктора
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

        /// <summary>
        /// Gets or sets the complete collection of users loaded from the database.
        /// </summary>
        /// <value>Observable collection containing all active users with their role information</value>
        /// <remarks>
        /// This collection is populated by LoadUsersAsync and serves as the source
        /// for applying filters to create the FilteredUsers collection.
        /// </remarks>
        public ObservableCollection<UserListItem> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filtered collection of users based on current search and role filters.
        /// </summary>
        /// <value>Observable collection containing users that match current filter criteria</value>
        /// <remarks>
        /// This collection is bound to the UI and automatically updates when filters change.
        /// It represents a subset of the Users collection based on SelectedRoleFilter and SearchText.
        /// </remarks>
        public ObservableCollection<UserListItem> FilteredUsers
        {
            get => _filteredUsers;
            set
            {
                _filteredUsers = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of available role filter options.
        /// </summary>
        /// <value>Observable collection containing "All" plus all available role names</value>
        /// <remarks>
        /// This collection is populated during initialization and remains static.
        /// It includes "All" as the default option plus all roles defined in RoleConstants.
        /// </remarks>
        public ObservableCollection<string> RoleFilterOptions { get; }

        /// <summary>
        /// Gets or sets the currently selected role filter.
        /// </summary>
        /// <value>String representing the selected role filter ("All" or specific role name)</value>
        /// <remarks>
        /// Setting this property automatically triggers filter reapplication through ApplyFilters().
        /// Default value is "All" which shows users with any role.
        /// </remarks>
        public string SelectedRoleFilter
        {
            get => _selectedRoleFilter;
            set
            {
                _selectedRoleFilter = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        /// <summary>
        /// Gets or sets the search text for filtering users.
        /// </summary>
        /// <value>String used to search across user names, emails, and roles</value>
        /// <remarks>
        /// Setting this property automatically triggers filter reapplication through ApplyFilters().
        /// Search is case-insensitive and matches against FullName, Email, and RolesDisplay.
        /// </remarks>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether initial user loading is in progress.
        /// </summary>
        /// <value>True if loading users for the first time; otherwise, false</value>
        /// <remarks>
        /// This property is used to show/hide loading indicators in the UI during initial data load.
        /// It's distinct from IsRefreshing which is used for pull-to-refresh operations.
        /// </remarks>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a refresh operation is in progress.
        /// </summary>
        /// <value>True if refresh is in progress; otherwise, false</value>
        /// <remarks>
        /// This property is used to control pull-to-refresh UI indicators.
        /// It's distinct from IsLoading which is used for initial data loading.
        /// </remarks>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has admin privileges.
        /// </summary>
        /// <value>True if the current user is an admin; otherwise, false</value>
        /// <remarks>
        /// This property is used for conditional UI display and access control.
        /// Only admin users can access the user list functionality.
        /// </remarks>
        public bool IsAdmin => _authenticationService.HasRole(RoleConstants.Admin);

        /// <summary>Gets the command for loading users from the database</summary>
        /// <value>Command that executes LoadUsersAsync when invoked</value>
        public ICommand LoadUsersCommand { get; }

        /// <summary>Gets the command for refreshing the user list</summary>
        /// <value>Command that executes RefreshUsersAsync when invoked</value>
        public ICommand RefreshCommand { get; }

        /// <summary>Gets the command for handling user selection</summary>
        /// <value>Command that navigates to user detail page when a user is selected</value>
        public ICommand UserSelectedCommand { get; }

        /// <summary>Gets the command for creating a new user</summary>
        /// <value>Command that navigates to user creation page when invoked</value>
        public ICommand CreateUserCommand { get; }

        /// <summary>
        /// Loads users from the database asynchronously with role information.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <remarks>
        /// This method:
        /// - Checks admin privileges and redirects if unauthorized
        /// - Loads active users with their roles from the database
        /// - Orders users by first name, then last name
        /// - Creates UserListItem objects with formatted role information
        /// - Applies current filters to populate FilteredUsers
        /// - Handles exceptions gracefully with debug logging
        /// </remarks>
        /// <exception cref="UnauthorizedAccessException">May be thrown if user loses admin privileges during execution</exception>
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
                // Використовуємо репозиторій замість прямого звернення до context
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

        /// <summary>
        /// Refreshes the user list by reloading data from the database.
        /// </summary>
        /// <returns>A task representing the asynchronous refresh operation</returns>
        /// <remarks>
        /// This method is typically called by pull-to-refresh gestures in the UI.
        /// It sets IsRefreshing to true during the operation and calls LoadUsersAsync
        /// to reload the data.
        /// </remarks>
        private async Task RefreshUsersAsync()
        {
            IsRefreshing = true;
            await LoadUsersAsync();
            IsRefreshing = false;
        }

        /// <summary>
        /// Applies current search and role filters to the Users collection.
        /// </summary>
        /// <remarks>
        /// This method:
        /// - Filters users by selected role (if not "All")
        /// - Applies search text filter across FullName, Email, and RolesDisplay
        /// - Updates FilteredUsers collection with matching results
        /// - Uses case-insensitive string matching for search
        /// 
        /// Called automatically when SelectedRoleFilter or SearchText properties change.
        /// </remarks>
        private void ApplyFilters()
        {
            var filtered = Users.AsEnumerable();

            // Apply role filter
            if (SelectedRoleFilter != "All")
            {
                filtered = filtered.Where(u => u.Roles.Contains(SelectedRoleFilter));
            }

            // Apply search filter
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

        /// @brief Navigates to the dashboard page
        /// @return A task representing the asynchronous navigation operation
        [RelayCommand]
        private async Task NavigateToDashboardAsync()
        {
            await _navigationService.NavigateToAsync("MainPage");
        }

        /// <summary>
        /// Navigates to the user detail page for the specified user.
        /// </summary>
        /// <param name="user">The user to view details for</param>
        /// <returns>A task representing the asynchronous navigation operation</returns>
        /// <remarks>
        /// This method is called when a user is selected from the list.
        /// It navigates to the UserDetailPage with the user's ID as a parameter.
        /// If the user parameter is null, no navigation occurs.
        /// </remarks>
        private async Task NavigateToUserDetailAsync(UserListItem user)
        {
            if (user != null)
            {
                await _navigationService.NavigateToAsync($"UserDetailPage?userId={user.Id}");
            }
        }

        /// <summary>
        /// Navigates to the user creation page.
        /// </summary>
        /// <returns>A task representing the asynchronous navigation operation</returns>
        /// <remarks>
        /// This method navigates to the UserDetailPage with userId=0 to indicate
        /// that a new user should be created rather than editing an existing one.
        /// </remarks>
        private async Task NavigateToCreateUserAsync()
        {
            await _navigationService.NavigateToAsync("UserDetailPage?userId=0");
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>
        /// This event is part of the INotifyPropertyChanged interface and is used
        /// by the data binding system to update the UI when property values change.
        /// </remarks>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed. If null, the caller member name is used.</param>
        /// <remarks>
        /// This method is called by property setters to notify the UI of property changes.
        /// The CallerMemberName attribute automatically provides the property name when called from a property setter.
        /// </remarks>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents a user item in the user list with display-friendly properties.
    /// </summary>
    /// <remarks>
    /// This class serves as a data transfer object for displaying user information
    /// in the user list. It includes formatted properties like FullName and RolesDisplay
    /// for easy binding to UI elements.
    /// </remarks>
    public class UserListItem
    {
        /// <summary>Gets or sets the unique identifier for the user</summary>
        /// <value>Integer representing the user's primary key in the database</value>
        public int Id { get; set; }

        /// <summary>Gets or sets the user's first name</summary>
        /// <value>String containing the user's first name, defaults to empty string</value>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's last name</summary>
        /// <value>String containing the user's last name, defaults to empty string</value>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's email address</summary>
        /// <value>String containing the user's email, defaults to empty string</value>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's full name (combination of first and last name)</summary>
        /// <value>String containing the formatted full name, defaults to empty string</value>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Gets or sets the date and time when the user was created</summary>
        /// <value>DateTime representing when the user account was created</value>
        public DateTime CreatedAt { get; set; }

        /// <summary>Gets or sets a value indicating whether the user account is active</summary>
        /// <value>True if the user account is active; otherwise, false</value>
        public bool IsActive { get; set; }

        /// <summary>Gets or sets the collection of role names assigned to the user</summary>
        /// <value>List of strings containing the names of roles assigned to the user</value>
        /// <remarks>Only includes active role assignments</remarks>
        public List<string> Roles { get; set; } = new();

        /// <summary>Gets or sets the display-friendly string of user roles</summary>
        /// <value>Comma-separated string of role names for display purposes</value>
        /// <remarks>
        /// This property is populated by joining the Roles collection with commas
        /// and is used for display in the UI and search functionality.
        /// </remarks>
        public string RolesDisplay { get; set; } = string.Empty;
    }
}