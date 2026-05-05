/*
 * @file RentalStateFactory.cs
 * @brief Factory to resolve state strategies based on string status
 */

namespace RentalApp.Database.States;

/// <summary>
/// Maps database string status to concrete IRentalState implementations.
/// </summary>
public static class RentalStateFactory
{
    public static IRentalState GetState(string status) => status switch
    {
        "Requested" => new RequestedState(),
        "Approved" => new ApprovedState(),
        "OutForRent" => new OutForRentState(),
        "Returned" => new ReturnedState(),
        _ => new DeniedState()
    };
}

// Internal fallback state
internal class DeniedState : BaseRentalState { }
