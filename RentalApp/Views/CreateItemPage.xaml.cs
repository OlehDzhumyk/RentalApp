/*
 * @file CreateItemPage.xaml.cs
 * @brief Code-behind for the item creation screen
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>
/// Interaction logic for CreateItemPage.
/// </summary>
public partial class CreateItemPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the CreateItemPage with its ViewModel.
    /// </summary>
    /// <param name="viewModel">The injected CreateItemViewModel.</param>
    public CreateItemPage(CreateItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}