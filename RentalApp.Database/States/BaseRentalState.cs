/*
 * @file BaseRentalState.cs
 * @brief Abstract base providing default behavior for rental states
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.States;

/// <summary>
/// Provides a base implementation where all transitions are denied by default.
/// Concrete states override only the transitions allowed for that specific status.
/// </summary>
public abstract class BaseRentalState : IRentalState
{
    public virtual Task<bool> ApproveAsync(Rental rental) => Task.FromResult(false);
    public virtual Task<bool> RejectAsync(Rental rental) => Task.FromResult(false);
    public virtual Task<bool> CollectAsync(Rental rental) => Task.FromResult(false);
    public virtual Task<bool> ReturnAsync(Rental rental) => Task.FromResult(false);
    public virtual Task<bool> FinalizeAsync(Rental rental) => Task.FromResult(false);
}
