/*
 * @file DbInitializer.cs
 * @brief Seeds the database with demo users and items
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RentalApp.Database.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace RentalApp.Database.Data;

/// <summary>
/// Populates an empty database with demo data so the app has something to show.
/// Run it through the migrations project: <c>dotnet run --project RentalApp.Migrations -- --seed</c>.
/// </summary>
public static class DbInitializer
{
    public const string AdminEmail = "admin@example.com";
    public const string UserEmail = "sam@example.com";
    public const string DemoPassword = "Password123";

    /// <summary>
    /// Adds an admin, an ordinary user and a handful of items around Edinburgh.
    /// </summary>
    /// <returns>True if data was added, false if the database already had users.</returns>
    public static async Task<bool> SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return false;

        var adminRole = await context.Roles.SingleAsync(r => r.Name == RoleConstants.Admin);
        var userRole = await context.Roles.SingleAsync(r => r.Name == RoleConstants.User);

        var admin = CreateUser("Morgan", "Reid", AdminEmail);
        var sam = CreateUser("Sam", "Taylor", UserEmail);
        context.Users.AddRange(admin, sam);
        await context.SaveChangesAsync();

        context.UserRoles.AddRange(
            new UserRole(admin.Id, adminRole.Id),
            new UserRole(admin.Id, userRole.Id),
            new UserRole(sam.Id, userRole.Id));

        context.Items.AddRange(
            CreateItem(admin, "Hilti hammer drill", "Heavy-duty SDS drill with a set of masonry bits.", 25m, -3.1883, 55.9533),
            CreateItem(admin, "Mountain bike", "Medium frame, front suspension. Helmet included.", 15m, -3.1591, 55.9444),
            CreateItem(sam, "Two-person tent", "Lightweight tent, packs down small. Used twice.", 8m, -3.2008, 55.9396),
            CreateItem(sam, "Pressure washer", "Good for patios and cars. Comes with a 10 m hose.", 12m, -3.2148, 55.9622),
            CreateItem(sam, "Projector", "1080p projector with HDMI cable and a pull-down screen.", 18m, -3.1747, 55.9768),
            CreateItem(admin, "Kayak", "Sit-on-top kayak with paddle and buoyancy aid.", 30m, -3.0790, 55.9500));

        await context.SaveChangesAsync();
        return true;
    }

    private static User CreateUser(string firstName, string lastName, string email)
    {
        var salt = BCryptNet.GenerateSalt();
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordSalt = salt,
            PasswordHash = BCryptNet.HashPassword(DemoPassword, salt),
        };
    }

    private static Item CreateItem(User owner, string title, string description, decimal pricePerDay,
                                   double longitude, double latitude) => new()
    {
        Title = title,
        Description = description,
        PricePerDay = pricePerDay,
        Owner = owner,
        Location = new Point(longitude, latitude) { SRID = 4326 },
    };
}
