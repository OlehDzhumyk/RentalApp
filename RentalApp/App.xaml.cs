using RentalApp.ViewModels;

namespace RentalApp;

public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;
	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		InitializeComponent();

		Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
		Routing.RegisterRoute(nameof(Views.UserListPage), typeof(Views.UserListPage));
		Routing.RegisterRoute(nameof(Views.UserDetailPage), typeof(Views.UserDetailPage));
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_serviceProvider.GetRequiredService<AppShell>());
	}
}
