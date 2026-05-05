/*
 * @file ReturnedState.cs
 * @brief Logic for rentals where the item has been physically returned
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.States;

/// <summary>
/// Handles the state after the item is returned but before final administrative closure.
/// </summary>
public class ReturnedState : BaseRentalState
{
    /// <summary>
    /// Finalizes the rental process, closing the transaction.
    /// </summary>
    /// <param name="rental">The rental entity to update.</param>
    /// <returns>True if the transition is valid.</returns>
    public override Task<bool> FinalizeAsync(Rental rental)
    {
        rental.Status = "Completed";
        return Task.FromResult(true);
    }
}
