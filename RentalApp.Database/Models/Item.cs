using NetTopologySuite.Geometries;

namespace RentalApp.Database.Models;

public class Item
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }

    public Point? Location { get; set; }

    public int OwnerId { get; set; }
    public virtual User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAvailable { get; set; } = true;
}