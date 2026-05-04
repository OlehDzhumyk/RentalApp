/*
 * @file BaseIntegrationTest.cs
 * @brief Infrastructure for integration tests using a real DI container and PostgreSQL
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Data;
using RentalApp.Database.Repositories;

namespace RentalApp.Test.Integration;

public abstract class BaseIntegrationTest : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly AppDbContext Context;

    protected BaseIntegrationTest()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        var services = new ServiceCollection();
        var connectionString = configuration.GetConnectionString("TestConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, x => x.UseNetTopologySuite()));

        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();

        ServiceProvider = services.BuildServiceProvider();
        Context = ServiceProvider.GetRequiredService<AppDbContext>();

        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
