/*
 * @file CreateItemViewModel.cs
 * @brief ViewModel for creating new rental items
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// Handles the logic for the "Create Item" screen.
/// Collects user input and persists new items via IItemRepository.
/// </summary>
public partial class CreateItemViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal PricePerDay { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateItemViewModel.
    /// </summary>
    public CreateItemViewModel(
        IItemRepository itemRepository,
        IAuthenticationService authService,
        INavigationService navigationService)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        base.Title = "List New Item";
    }

    /// <summary>
    /// Validates input and saves the new item to the database.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title) || PricePerDay <= 0)
        {
            SetError("Please provide a valid title and price.");
            return;
        }

        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            SetError("You must be logged in to create an item.");
            return;
        }

        IsBusy = true;
        try
        {
            var newItem = new Item
            {
                Title = Title.Trim(),
                Description = Description.Trim(),
                PricePerDay = PricePerDay,
                OwnerId = currentUser.Id,
                CreatedAt = DateTime.UtcNow,
                IsAvailable = true
            };

            await _itemRepository.AddAsync(newItem);
            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to save item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}