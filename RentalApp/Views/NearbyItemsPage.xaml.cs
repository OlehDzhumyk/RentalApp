/*
 * @file NearbyItemsPage.xaml.cs
 * @brief Code-behind for the location-based search screen
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class NearbyItemsPage : ContentPage
{
    private readonly NearbyItemsViewModel _viewModel;

    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Automatically trigger search when user navigates to the page
        if (_viewModel.NearbyItems.Count == 0)
        {
            await _viewModel.LoadNearbyItemsCommand.ExecuteAsync(null);
        }
    }
}