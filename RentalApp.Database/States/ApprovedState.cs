/*
 * @file ApprovedState.cs
 * @brief State implementation for an approved rental ready for pickup
 */
using RentalApp.Database.Models;
using RentalApp.Database.States.Interfaces;

namespace RentalApp.Database.States;

public class ApprovedState : IRentalState
{
    public string StateName => "Approved";

    public Task<bool> ApproveAsync(Rental rental) => Task.FromResult(false);
    public Task<bool> RejectAsync(Rental rental) => Task.FromResult(false);

    public Task<bool> StartRentalAsync(Rental rental)
    {
        rental.Status = "OutForRent";
        return Task.FromResult(true);
    }

    public Task<bool> ReturnItemAsync(Rental rental) => Task.FromResult(false);
}
