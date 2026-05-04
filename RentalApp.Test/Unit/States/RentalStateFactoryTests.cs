/*
 * @file RentalStateFactoryTests.cs
 * @brief Tests for the RentalStateFactory logic
 */
using RentalApp.Database.States;

namespace RentalApp.Test.States;

public class RentalStateFactoryTests
{
    [Theory]
    [InlineData("Requested", typeof(RequestedState))]
    [InlineData("Approved", typeof(ApprovedState))]
    public void GetState_ShouldReturnCorrectType(string status, Type expectedType)
    {
        // Act
        var state = RentalStateFactory.GetState(status);

        // Assert
        Assert.IsType(expectedType, state);
    }
}
