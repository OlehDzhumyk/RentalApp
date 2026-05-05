/*
 * @file RentalService.cs
 * @brief Implementation of rental management logic using the State Pattern
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

    public async Task<bool> CanRentItemAsync(int itemId, DateTime start, DateTime end)
    {
        var existingRentals = await _rentalRepository.GetByItemIdAsync(itemId);
        return !existingRentals.Any(r =>
            (r.Status == "Approved" || r.Status == "OutForRent") &&
            r.StartDate < end &&
            r.EndDate > start);
    }

    public async Task<Rental> RequestRentalAsync(int itemId, int borrowerId, DateTime start, DateTime end)
    {
        var item = await _itemRepository.GetByIdAsync(itemId);
        if (item == null) throw new ArgumentException("Item not found");

        if (!await CanRentItemAsync(itemId, start, end))
            throw new InvalidOperationException("Item is not available for selected dates.");

        var rental = new Rental
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            StartDate = start,
            EndDate = end,
            Status = "Requested",
            TotalPrice = (decimal)Math.Max(1, (end - start).TotalDays) * item.PricePerDay
        };

        return await _rentalRepository.CreateAsync(rental);
    }

    /// <summary>
    /// Progresses the rental to the next logical state based on the requested action.
    /// </summary>
    public async Task<bool> TransitionAsync(int rentalId, Func<IRentalState, Rental, Task<bool>> action)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId);
        if (rental == null) return false;

        var state = RentalStateFactory.GetState(rental.Status);
        bool success = await action(state, rental);

        if (success)
        {
            await _rentalRepository.UpdateAsync(rental);
        }

        return success;
    }
}
