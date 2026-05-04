/*
 * @file RentalService.cs
 * @brief Implementation of rental management logic including price calculation
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Database.States;

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

    /// <inheritdoc/>
    public async Task<bool> CanRentItemAsync(int itemId, DateTime start, DateTime end)
    {
        var existingRentals = await _rentalRepository.GetByItemIdAsync(itemId);

        // Logical check for overlapping dates with active rentals
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

    /// <summary>
    /// Updates the status of a rental using the State Pattern logic.
    /// </summary>
    public async Task<bool> ApproveRentalAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId);
        if (rental == null) return false;

        // Factory returns the strategy based on current status string
        var state = RentalStateFactory.GetState(rental.Status);

        bool success = await state.ApproveAsync(rental);

        if (success)
        {
            await _rentalRepository.UpdateAsync(rental);
        }

        return success;
    }
}
