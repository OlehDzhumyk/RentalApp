using RentalApp.ViewModels;
using RentalApp.Views;

namespace RentalApp;

public partial class AppShell : Shell
{
	public AppShell(AppShellViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();

        Routing.RegisterRoute(nameof(ItemsListPage), typeof(ItemsListPage));
        Routing.RegisterRoute(nameof(CreateItemPage), typeof(CreateItemPage));
        Routing.RegisterRoute(nameof(UserDetailPage), typeof(UserDetailPage));
        Routing.RegisterRoute(nameof(NearbyItemsPage), typeof(NearbyItemsPage));
    }
}
