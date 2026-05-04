/*
 * @file CreateItemViewModel.cs
 * @brief ViewModel for creating items with automatic GPS tagging
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using Point = NetTopologySuite.Geometries.Point;

namespace RentalApp.ViewModels;

public partial class CreateItemViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly ILocationService _locationService; // Додано

    [ObservableProperty]
    public partial string ItemTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal PricePerDay { get; set; }

    /// <summary>
    /// Toggle to decide if we should attach current GPS coordinates to the item.
    /// </summary>
    [ObservableProperty]
    public partial bool UseCurrentLocation { get; set; } = true;

    public CreateItemViewModel(
        IItemRepository itemRepository,
        IAuthenticationService authService,
        INavigationService navigationService,
        ILocationService locationService) // Додано
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));

        Title = "List New Item";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ItemTitle) || PricePerDay <= 0)
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
            Point? itemLocation = null;

            if (UseCurrentLocation)
            {
                var coords = await _locationService.GetCurrentLocationAsync();
                if (coords != null)
                {
                    // NTS Point: (X = Longitude, Y = Latitude)
                    itemLocation = new Point(coords.Value.Longitude, coords.Value.Latitude) { SRID = 4326 };
                }
            }

            var newItem = new Item
            {
                Title = ItemTitle.Trim(),
                Description = Description.Trim(),
                PricePerDay = PricePerDay,
                OwnerId = currentUser.Id,
                CreatedAt = DateTime.UtcNow,
                IsAvailable = true,
                Location = itemLocation // Тепер річ має координати!
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