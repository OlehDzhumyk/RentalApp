/*
 * @file ItemsListPage.xaml.cs
 * @brief Code-behind for the items gallery screen
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>
/// Interaction logic for ItemsListPage.
/// Manages the lifecycle and initialization of the items gallery.
/// </summary>
public partial class ItemsListPage : ContentPage
{
    private readonly ItemsListViewModel _viewModel;

    public ItemsListPage(ItemsListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Triggers item loading when the page becomes visible.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.LoadItemsCommand.CanExecute(null))
        {
            _viewModel.LoadItemsCommand.Execute(null);
        }
    }
}