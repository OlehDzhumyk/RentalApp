/*
 * @file Rental.cs
 * @brief Entity representing a rental agreement between users
 * @author RentalApp Development Team
 * @date 2026
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

/// <summary>
/// Represents a rental transaction, tracking the lifecycle from request to completion.
/// </summary>
public class Rental
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ItemId { get; set; }
    public virtual Item Item { get; set; } = null!;

    [Required]
    public int BorrowerId { get; set; }
    public virtual User Borrower { get; set; } = null!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Current state of the rental as a string for database persistence.
    /// Standard flow: Requested -> Approved -> OutForRent -> Returned -> Completed.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Requested";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Helper to calculate the duration of the rental in days.
    /// </summary>
    [NotMapped]
    public int DurationDays => (EndDate - StartDate).Days > 0 ? (EndDate - StartDate).Days : 1;
}
