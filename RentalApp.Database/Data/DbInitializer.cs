/*
 * @file DbInitializer.cs
 * @brief Utility for seeding the database with development fixtures
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data;

/// <summary>
/// Provides methods to populate the database with initial sample data.
/// Primarily used for development and demonstration purposes.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with a test user and items located in Edinburgh.
    /// </summary>
    /// <param name="context">The database context instance.</param>
    public static async Task SeedAsync(AppDbContext context)
    {
        // Skip seeding if data already exists
        if (await context.Items.AnyAsync()) return;

        var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

        // 1. Ensure Roles are present (should be handled by migration, but checking for safety)
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null) return;

        // 2. Create Development User
        var testUser = new User
        {
            FirstName = "Oleh",
            LastName = "Dzhumyk",
            Email = "oleh.dzhumyk@test.com",
            PasswordHash = "AQAAAAEAACcQAAAAEBLT...", // Temporary dev hash
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        // 3. Assign Role
        context.UserRoles.Add(new UserRole { UserId = testUser.Id, RoleId = adminRole.Id });

        // 4. Add Sample Items (Coordinates set for Edinburgh, Scotland)
        context.Items.AddRange(
            new Item
            {
                Title = "Hilti Hammer Drill",
                Description = "Professional heavy-duty drill for construction.",
                PricePerDay = 25.00m,
                OwnerId = testUser.Id,
                IsAvailable = true,
                Location = gf.CreatePoint(new Coordinate(-3.1883, 55.9533)), // Edinburgh Centre
                CreatedAt = DateTime.UtcNow
            },
            new Item
            {
                Title = "Mountain Bike",
                Description = "Perfect for Arthur's Seat and local trails.",
                PricePerDay = 15.00m,
                OwnerId = testUser.Id,
                IsAvailable = true,
                Location = gf.CreatePoint(new Coordinate(-3.1591, 55.9444)), // Holyrood area
                CreatedAt = DateTime.UtcNow
            }
        );

        await context.SaveChangesAsync();
    }
}