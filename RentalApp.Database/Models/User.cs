/*
 * @file User.cs
 * @brief Represents a user entity in the database
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

/// <summary>
/// Entity representing a registered user in the system.
/// </summary>
[Table("users")]
[PrimaryKey(nameof(Id))]
public class User
{
    /// <summary>Unique identifier for the user.</summary>
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string PasswordSalt { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // --- Navigation Properties ---

    /// <summary>Roles assigned to this user.</summary>
    public List<UserRole> UserRoles { get; set; } = new();

    /// <summary>
    /// Items owned and listed for rent by this user.
    /// This resolves the Entity Framework configuration error.
    /// </summary>
    public List<Item> Items { get; set; } = new();

    /// <summary>Computed full name (not stored in DB).</summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}