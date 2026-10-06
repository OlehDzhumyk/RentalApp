/*
 * @file MigrationTests.cs
 * @brief Guards against model changes that were never turned into a migration
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;

namespace RentalApp.Test.Unit.Database;

public class MigrationTests
{
    [Fact]
    public void Model_ShouldMatchLatestMigration()
    {
        // Integration tests build the schema with EnsureCreated, which ignores migrations.
        // This compares the model with the migrations snapshot instead (no database needed).
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=unused", o => o.UseNetTopologySuite().MigrationsAssembly("RentalApp.Migrations"))
            .Options;
        using var context = new AppDbContext(options);

        Assert.False(context.Database.HasPendingModelChanges(),
            "The model has changed since the last migration. Run 'dotnet ef migrations add <Name> --project RentalApp.Migrations'.");
    }
}
