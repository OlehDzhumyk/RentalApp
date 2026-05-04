/*
 * @file AppDbContext.cs
 * @brief Entity Framework Core database context configuration
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Models;
using System.Reflection;

namespace RentalApp.Database.Data;

/// <summary>
/// Main database context for the application.
/// Manages connection strings, entity configurations, and PostGIS integration.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("RentalApp.Database.appsettings.json");

        if (stream == null)
            throw new InvalidOperationException("Embedded appsettings.json not found in the assembly.");

        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var connectionString = config.GetConnectionString("DevelopmentConnection");

        // Configures PostgreSQL connection with NetTopologySuite for spatial queries (PostGIS)
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.UseNetTopologySuite();
            options.MigrationsAssembly("RentalApp.Migrations");
        });
    }

    #region DbSets

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Item> Items { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable PostGIS extension for spatial data operations (e.g., finding nearby items)
        modelBuilder.HasPostgresExtension("postgis");

        // User Entity Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
        });

        // Item Entity Configuration (Tier 1)
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PricePerDay).HasPrecision(18, 2);

            // One-to-Many Relationship: User -> Items
            entity.HasOne(i => i.Owner)
                  .WithMany(u => u.Items)
                  .HasForeignKey(i => i.OwnerId)
                  .OnDelete(DeleteBehavior.Cascade);

            // GIST index is crucial for fast spatial queries in PostGIS
            entity.HasIndex(e => e.Location).HasMethod("GIST");
        });

        // UserRole Many-to-Many Entity Configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            entity.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });
    }
}