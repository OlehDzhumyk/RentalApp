/*
 * @file IRentalState.cs
 * @brief Interface defining available transitions for a rental entity
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.States;

/// <summary>
/// Defines the contract for rental state transitions within the State Pattern.
/// Each method represents a business action that changes the rental lifecycle.
/// </summary>
public interface IRentalState
{
    Task<bool> ApproveAsync(Rental rental);
    Task<bool> RejectAsync(Rental rental);
    Task<bool> CollectAsync(Rental rental);
    Task<bool> ReturnAsync(Rental rental);
    Task<bool> FinalizeAsync(Rental rental);
}
