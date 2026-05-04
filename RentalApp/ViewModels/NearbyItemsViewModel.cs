/*
 * @file NearbyItemsViewModel.cs
 * @brief ViewModel for searching items based on user location
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// <summary>
/// Manages the discovery of items near the user's current physical location.
/// Integrates GPS services with spatial database queries.
/// </summary>
public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    public partial ObservableCollection<Item> NearbyItems { get; set; } = new();

    [ObservableProperty]
    public partial double SearchRadius { get; set; } = 5.0; // Default 5km

    public NearbyItemsViewModel(IItemRepository itemRepository, ILocationService locationService)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));

        Title = "Find Near Me";
    }

    /// <summary>
    /// Retrieves the current location and searches for items within the defined radius.
    /// </summary>
    [RelayCommand]
    private async Task LoadNearbyItemsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ClearError();

        try
        {
            var location = await _locationService.GetCurrentLocationAsync();

            if (location == null)
            {
                SetError("Could not retrieve your location. Please check GPS settings.");
                return;
            }

            var result = await _itemRepository.GetNearbyAsync(
                location.Value.Latitude,
                location.Value.Longitude,
                SearchRadius);

            NearbyItems = new ObservableCollection<Item>(result);
        }
        catch (Exception ex)
        {
            SetError($"Error searching items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}