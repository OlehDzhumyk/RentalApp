/*
 * @file NearbyItemsViewModel.cs
 * @brief ViewModel for location-based item discovery with configurable radius
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

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    public partial ObservableCollection<NearbyItem> NearbyItems { get; set; } = new();

    [ObservableProperty]
    public partial double SearchRadius { get; set; } = 5.0;

    public NearbyItemsViewModel(IItemRepository itemRepository, ILocationService locationService)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        Title = "Items Near Me";
    }

    /// <summary>
    /// Fetches items within SearchRadius of the device, nearest first.
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

            var items = await _itemRepository.GetNearbyAsync(
                location.Value.Latitude,
                location.Value.Longitude,
                SearchRadius);

            NearbyItems = new ObservableCollection<NearbyItem>(items);
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
