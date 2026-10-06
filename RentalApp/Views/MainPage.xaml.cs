using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private readonly MainViewModel _viewModel;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadUserData();
    }
}