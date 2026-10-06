namespace RentalApp.Database.Models;

/// <summary>
/// An item returned by a proximity search, with its distance from the search point as calculated by PostGIS.
/// </summary>
public record NearbyItem(Item Item, double DistanceKm);
