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
        builder.Services.AddDbContext<AppDbContext>();

        // --- Core Services ---
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddScoped<IRentalService, RentalService>();

        // --- Repositories ---
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IItemRepository, ItemRepository>();
        builder.Services.AddScoped<IRentalRepository, RentalRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();

        // --- ViewModels ---
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<NearbyItemsViewModel>();
        builder.Services.AddSingleton<AppShellViewModel>();

        // --- Views ---
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<NearbyItemsPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

#if DEBUG
        SeedDatabase(app);
#endif

        return app;
    }

    private static void SeedDatabase(MauiApp app)
    {
        // Execute seeding without blocking the main thread
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await DbInitializer.SeedAsync(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database seeding failed: {ex.Message}");
            }
        });
    }
}
