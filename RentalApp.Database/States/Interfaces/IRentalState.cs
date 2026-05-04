/*
 * @file IRentalState.cs
 * @brief Interface for the Rental State Pattern implementation
 * @author RentalApp Development Team
 * @date 2026
 */


/*
 * @file IRentalState.cs
 * @brief Interface for the Rental State Pattern implementation
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.Models;

namespace RentalApp.Database.States.Interfaces
{

    /// <summary>
    /// Defines the behavior for each specific state in the rental workflow.
    /// </summary>
    public interface IRentalState
    {
        string StateName { get; }
        Task<bool> ApproveAsync(Rental rental);
        Task<bool> RejectAsync(Rental rental);
        Task<bool> StartRentalAsync(Rental rental);
        Task<bool> ReturnItemAsync(Rental rental);
    }
}
