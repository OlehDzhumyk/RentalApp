/*
 * @file NearbyItemsViewModel.cs
 * @brief Enhanced ViewModel with distance calculation metrics
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using Point = NetTopologySuite.Geometries.Point;

namespace RentalApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    public partial ObservableCollection<ItemDisplayWrapper> NearbyItems { get; set; } = new();

    public NearbyItemsViewModel(IItemRepository itemRepository, ILocationService locationService)
    {
        _itemRepository = itemRepository;
        _locationService = locationService;
        Title = "Items Near Me";
    }

    [RelayCommand]
    private async Task LoadNearbyItemsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var location = await _locationService.GetCurrentLocationAsync();
            if (location == null)
            {
                SetError("GPS location unavailable.");
                return;
            }

            var items = await _itemRepository.GetNearbyAsync(location.Value.Latitude, location.Value.Longitude, 10.0);

            var userPoint = new Point(location.Value.Longitude, location.Value.Latitude) { SRID = 4326 };

            var wrappedItems = items.Select(item => new ItemDisplayWrapper(item, userPoint));
            NearbyItems = new ObservableCollection<ItemDisplayWrapper>(wrappedItems);
        }
        catch (Exception ex)
        {
            SetError($"Search failed: {ex.Message}");
        }
        finally { IsBusy = false; }
    }
}

/// <summary>
/// Wrapper to include distance metrics without modifying the core Item entity.
/// </summary>
public class ItemDisplayWrapper
{
    public Item Item { get; }
    public double DistanceKm { get; }
    public string FormattedDistance => $"{DistanceKm:F1} km away";

    public ItemDisplayWrapper(Item item, Point userLocation)
    {
        Item = item;
        if (item.Location != null)
        {
            // Haversine or simple NTS distance (PostGIS/NTS handles this in meters usually)
            // For SRID 4326, distance is in degrees, so we use a coordinate calculator or 
            // assume the repository already sorted/filtered them.
            // For UI, we'll use a simple approximation for now:
            DistanceKm = item.Location.Distance(userLocation) * 111.1; // Very rough degree-to-km conversion
        }
    }
}