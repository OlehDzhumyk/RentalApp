/*
 * @file Program.cs
 * @brief Applies EF Core migrations and optionally seeds demo data
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Migrations;

// Usage: dotnet run --project RentalApp.Migrations [-- --seed]
await using var context = new AppDbContextFactory().CreateDbContext(args);

Console.WriteLine("Applying migrations...");
await context.Database.MigrateAsync();
Console.WriteLine("Database is up to date.");

if (args.Contains("--seed"))
{
    var seeded = await DbInitializer.SeedAsync(context);
    Console.WriteLine(seeded
        ? $"Demo data added. Log in as {DbInitializer.AdminEmail} or {DbInitializer.UserEmail} with password '{DbInitializer.DemoPassword}'."
        : "Demo data already present, nothing to add.");
}
