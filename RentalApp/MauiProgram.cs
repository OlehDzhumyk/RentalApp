/*
 * @file MauiProgram.cs
 * @brief Application entry point and service registration
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.Extensions.Logging;
using RentalApp.Database.Data;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using RentalApp.ViewModels;
using RentalApp.Views;

namespace RentalApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- Infrastructure & Hardware ---
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        // MAUI never creates DI scopes, so "scoped" would mean one DbContext for the whole app.
        // Transient gives each view model its own short-lived context instead.
        builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);

        // --- Core Services ---
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddTransient<IRentalService, RentalService>();

        // --- Repositories ---
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<IItemRepository, ItemRepository>();
        builder.Services.AddTransient<IRentalRepository, RentalRepository>();
        builder.Services.AddTransient<IRoleRepository, RoleRepository>();

        // --- ViewModels ---
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<NearbyItemsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddSingleton<AppShellViewModel>();

        // --- Views ---
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<NearbyItemsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
