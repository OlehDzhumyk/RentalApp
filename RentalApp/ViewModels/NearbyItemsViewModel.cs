/*
 * @file NearbyItemsViewModel.cs
 * @brief ViewModel for location-based item discovery with configurable radius
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetTopologySuite.Geometries;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using System.Collections.ObjectModel;
using Point = NetTopologySuite.Geometries.Point;

namespace RentalApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    public partial ObservableCollection<ItemDisplayWrapper> NearbyItems { get; set; } = new();

    [ObservableProperty]
    public partial double SearchRadius { get; set; } = 5.0;

    public NearbyItemsViewModel(IItemRepository itemRepository, ILocationService locationService)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        Title = "Items Near Me";
    }

    /// <summary>
    /// Fetches items within the SearchRadius and calculates distance for each.
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

            var userPoint = new Point(location.Value.Longitude, location.Value.Latitude) { SRID = 4326 };

            var wrappedItems = items.Select(item => new ItemDisplayWrapper(item, userPoint));
            NearbyItems = new ObservableCollection<ItemDisplayWrapper>(wrappedItems);
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

/// <summary>
/// Domain-specific wrapper to calculate and format distance for the UI.
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
            // Simple approximation for distance in KM using NTS
            // In a production app, we would use Haversine or let PostGIS return the distance
            DistanceKm = item.Location.Distance(userLocation) * 111.1;
        }
    }
}