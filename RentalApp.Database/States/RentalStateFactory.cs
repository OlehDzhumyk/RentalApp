/*
 * @file RentalStateFactory.cs
 * @brief Factory for instantiating rental state handlers
 * @author RentalApp Development Team
 * @date 2026
 */

using RentalApp.Database.States.Interfaces;

namespace RentalApp.Database.States;

/// <summary>
/// Factory to create the appropriate IRentalState instance based on the status string.
/// </summary>
public static class RentalStateFactory
{
    /// <summary>
    /// Returns the state handler for a given status string.
    /// </summary>
    /// <param name="status">The status string from the database.</param>
    /// <returns>An implementation of IRentalState.</returns>
    public static IRentalState GetState(string status)
    {
        return status switch
        {
            "Requested" => new RequestedState(),
            "Approved" => new ApprovedState(),
            "Rejected" => new RejectedState(),
            _ => new RequestedState()
        };
    }
}
