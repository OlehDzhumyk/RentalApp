/*
 * @file OutForRentState.cs
 * @brief Logic for rentals that are currently with the borrower
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.States;

/// <summary>
/// Handles the state where the item has been collected by the borrower.
/// Only allows the transition to 'Returned'.
/// </summary>
public class OutForRentState : BaseRentalState
{
    /// <summary>
    /// Processes the return of the item from the borrower.
    /// </summary>
    /// <param name="rental">The rental entity to update.</param>
    /// <returns>True if the transition is valid.</returns>
    public override Task<bool> ReturnAsync(Rental rental)
    {
        rental.Status = "Returned";
        return Task.FromResult(true);
    }
}
