/*
 * @file BaseIntegrationTest.cs
 * @brief Base class for integration tests providing a clean database state
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Data;

namespace RentalApp.Test;

public abstract class BaseIntegrationTest : IDisposable
{
    protected readonly AppDbContext Context;

    protected BaseIntegrationTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        var connectionString = configuration.GetConnectionString("TestConnection");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, x => x.UseNetTopologySuite())
            .Options;

        Context = new AppDbContext(options);

        // Ensure we start with a clean slate
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
