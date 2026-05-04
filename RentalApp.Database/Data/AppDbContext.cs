/*
 * @file AppDbContext.cs
 * @brief EF Core context with PostGIS Geography (meters) support
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Models;
using System.Reflection;

namespace RentalApp.Database.Data;

/// <summary>
/// Main database context for RentalApp.
/// Configures spatial data handling and persistence logic.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// Configures the database connection using embedded configuration.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        // Loading appsettings.json from embedded resources for portability
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("RentalApp.Database.appsettings.json");

        if (stream == null)
            throw new InvalidOperationException("Embedded appsettings.json not found. Ensure the file is set as EmbeddedResource.");

        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var connectionString = config.GetConnectionString("DevelopmentConnection");

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            // Enable NetTopologySuite for spatial types (Point, Geometry, etc.)
            options.UseNetTopologySuite();
            options.MigrationsAssembly("RentalApp.Migrations");
        });
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Rental> Rentals => Set<Rental>();

    /// <summary>
    /// Fluently configures the domain model constraints and spatial indexes.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Required: Register PostGIS extension in PostgreSQL
        modelBuilder.HasPostgresExtension("postgis");

        // --- User Configuration ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
        });

        // --- Item Configuration (Spatial Focus) ---
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PricePerDay).HasPrecision(18, 2);

            /* 
             * Using 'geography' instead of 'geometry'
             * Geography uses WGS 84 (SRID 4326) and measures distance in METERS.
             * This allows direct use of .Distance() <= 5000 (for 5km).
             */
            entity.Property(e => e.Location)
                  .HasColumnType("geography (point, 4326)");

            // Spatial index for high-performance distance queries
            entity.HasIndex(e => e.Location).HasMethod("GIST");
        });

        // --- Rental Configuration ---
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(r => r.Item)
                  .WithMany()
                  .HasForeignKey(r => r.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Borrower)
                  .WithMany()
                  .HasForeignKey(r => r.BorrowerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Identity & Roles ---
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
        });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", IsDefault = false },
            new Role { Id = 2, Name = "User", IsDefault = true }
        );
    }
}
