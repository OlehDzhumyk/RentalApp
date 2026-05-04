/*
 * @file AppDbContextFactory.cs
 * @brief Factory for creating DbContext at design time
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RentalApp.Database.Data;

namespace RentalApp.Migrations;

/// <summary>
/// Allows EF Core CLI tools to discover and instantiate AppDbContext.
/// This is required when the DbContext is in a separate library.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        return new AppDbContext(optionsBuilder.Options);
    }
}