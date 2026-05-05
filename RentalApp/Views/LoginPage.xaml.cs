/*
 * @file LoginPage.xaml.cs
 * @brief UI-behind logic for user authentication
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class LoginPage : ContentPage
{
    /// <summary>
    /// Initializes the page and binds the injected ViewModel.
    /// </summary>
    /// <param name="viewModel">The authenticated ViewModel instance provided by DI.</param>
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus();
    }
}
