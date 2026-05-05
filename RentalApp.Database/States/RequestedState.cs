namespace RentalApp.Database.States;

public class RequestedState : BaseRentalState
{
    public override Task<bool> ApproveAsync(Models.Rental rental)
    {
        rental.Status = "Approved";
        return Task.FromResult(true);
    }

    public override Task<bool> RejectAsync(Models.Rental rental)
    {
        rental.Status = "Rejected";
        return Task.FromResult(true);
    }
}
