namespace RentalApp.Database.States;

public class ApprovedState : BaseRentalState
{
    public override Task<bool> CollectAsync(Models.Rental rental)
    {
        rental.Status = "OutForRent";
        return Task.FromResult(true);
    }
}
