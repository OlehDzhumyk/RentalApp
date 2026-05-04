/*
 * @file MauiProgram.cs
 * @brief Application entry point and service registration
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

        // Database and Repositories
        builder.Services.AddDbContext<AppDbContext>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IItemRepository, ItemRepository>();

        // Services
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<NearbyItemsViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<CreateItemPage>();

        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Runtime Database Seeding (Optional & Conditional)
#if DEBUG
        Task.Run(async () =>
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbInitializer.SeedAsync(context);
        }).Wait();
#endif

        return app;
    }
}