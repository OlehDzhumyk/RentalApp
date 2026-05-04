/*
 * @file RentalService.cs
 * @brief Implementation of rental management logic including price calculation
 */

using RentalApp.Database.Models;
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

    public async Task<bool> CanRentItemAsync(int itemId, DateTime start, DateTime end)
    {
        var existingRentals = await _rentalRepository.GetByItemIdAsync(itemId);
        return !existingRentals.Any(r =>
            (r.Status == "Approved" || r.Status == "OutForRent") &&
            r.StartDate < end &&
            r.EndDate > start);
    }

    /// <summary>
    /// Creates a new rental request after validating availability and calculating total price.
    /// </summary>
    public async Task<Rental> RequestRentalAsync(int itemId, int borrowerId, DateTime start, DateTime end)
    {
        var item = await _itemRepository.GetByIdAsync(itemId);
        if (item == null) throw new ArgumentException("Item not found");

        if (!await CanRentItemAsync(itemId, start, end))
            throw new InvalidOperationException("Item is not available for the selected dates.");

        var rental = new Rental
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            StartDate = start,
            EndDate = end,
            Status = "Requested",
            // Logic: Duration in days * daily rate
            TotalPrice = (decimal)(end - start).TotalDays * item.PricePerDay
        };

        return await _rentalRepository.CreateAsync(rental);
    }
}
