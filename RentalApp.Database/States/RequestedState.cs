/*
 * @file RequestedState.cs
 * @brief Concrete state implementation for a new rental request
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.States;

/// <summary>
/// Handles behavior for rentals in the 'Requested' state.
/// </summary>
public class RequestedState : IRentalState
{
    public string StateName => "Requested";

    public Task<bool> ApproveAsync(Rental rental)
    {
        rental.Status = "Approved";
        return Task.FromResult(true);
    }

    public Task<bool> RejectAsync(Rental rental)
    {
        rental.Status = "Rejected";
        return Task.FromResult(true);
    }

    public Task<bool> StartRentalAsync(Rental rental) =>
        Task.FromException<bool>(new InvalidOperationException("Cannot start rental from Requested state. Must be Approved first."));

    public Task<bool> ReturnItemAsync(Rental rental) =>
        Task.FromException<bool>(new InvalidOperationException("Cannot return item that hasn't been rented."));
}
