/*
 * @file BaseIntegrationTest.cs
 * @brief Base class for integration tests providing a clean database state
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Data;

namespace RentalApp.Test.Integration; // Оновлений namespace

public abstract class BaseIntegrationTest : IDisposable
{
    protected readonly AppDbContext Context;

    protected BaseIntegrationTest()
    {
        // Вказуємо шлях до папки виконання, щоб знайти json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("TestConnection");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, x => x.UseNetTopologySuite())
            .Options;

        Context = new AppDbContext(options);

        // Повне очищення та перестворення схеми перед кожним тестом
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
