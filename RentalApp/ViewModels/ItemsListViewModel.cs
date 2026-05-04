/*
 * @file ItemsListViewModel.cs
 * @brief ViewModel for browsing available rental items
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using RentalApp.Views;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// <summary>
/// Manages the logic for displaying a list of available rental items.
/// </summary>
public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IItemRepository _itemRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    public partial ObservableCollection<Item> Items { get; set; } = new();

    public ItemsListViewModel(IItemRepository itemRepository, INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
        Title = "Browse Items";
    }


    [RelayCommand]
    private async Task NavigateToCreateItemAsync()
    {
        await _navigationService.NavigateToAsync(nameof(CreateItemPage));
    }

    /// <summary>
    /// Loads available items from the repository and updates the collection.
    /// </summary>
    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _itemRepository.GetAllAsync();
            Items = new ObservableCollection<Item>(result);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}