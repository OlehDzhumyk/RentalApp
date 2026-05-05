using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>
/// Interaction logic for LoginPage.xaml.
/// Handles UI-specific tasks such as focus management that are outside the scope of the ViewModel.
/// </summary>
public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Ensures the primary input field is focused when the page appears to improve user UX.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus();
    }
}
