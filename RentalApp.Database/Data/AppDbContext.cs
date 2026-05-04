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
            throw new InvalidOperationException("Embedded appsettings.json not found.");

        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var connectionString = config.GetConnectionString("DevelopmentConnection");

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.UseNetTopologySuite();
            options.MigrationsAssembly("RentalApp.Migrations");
        });
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Rental> Rentals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PricePerDay).HasPrecision(18, 2);
            entity.HasIndex(e => e.Location).HasMethod("GIST");
        });

        // Rental Entity Configuration
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

            // Relationships
            entity.HasOne(r => r.Item)
                  .WithMany()
                  .HasForeignKey(r => r.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Borrower)
                  .WithMany()
                  .HasForeignKey(r => r.BorrowerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

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
