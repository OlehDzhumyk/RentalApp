/*
 * @file ProfilePage.xaml.cs
 * @brief Code-behind for the profile screen
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
