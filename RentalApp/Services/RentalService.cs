/*
 * @file RentalService.cs
 * @brief Implementation of rental management logic
 */
using RentalApp.Database.Repositories;

namespace RentalApp.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IItemRepository _itemRepository;

    public RentalService(IRentalRepository rentalRepository, IItemRepository itemRepository)
    {
        _rentalRepository = rentalRepository;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// Checks if an item is available for rent during the specified period.
    /// Implementation of Tier 2: Date overlap validation logic.
    /// </summary>
    public async Task<bool> CanRentItemAsync(int itemId, DateTime start, DateTime end)
    {
        var existingRentals = await _rentalRepository.GetByItemIdAsync(itemId);

        // Logic: A rental overlaps if (StartA < EndB) AND (EndA > StartB)
        // We only care about Approved or OutForRent statuses
        return !existingRentals.Any(r =>
            (r.Status == "Approved" || r.Status == "OutForRent") &&
            r.StartDate < end &&
            r.EndDate > start);
    }
}
