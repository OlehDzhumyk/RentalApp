/*
 * @file RejectedState.cs
 * @brief State implementation for a rejected rental request
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;
using RentalApp.Database.States.Interfaces;

namespace RentalApp.Database.States;

public class RejectedState : IRentalState
{
    public string StateName => "Rejected";

    public Task<bool> ApproveAsync(Rental rental) => Task.FromResult(false);
    public Task<bool> RejectAsync(Rental rental) => Task.FromResult(false);
    public Task<bool> StartRentalAsync(Rental rental) => Task.FromResult(false);
    public Task<bool> ReturnItemAsync(Rental rental) => Task.FromResult(false);
}
