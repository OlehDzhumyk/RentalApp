/*
 * @file RegisterViewModelTests.cs
 * @brief Unit tests for RegisterViewModel validating form constraints and auth flow
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Test.Unit.ViewModels;

public class RegisterViewModelTests
{
    private readonly Mock<IAuthenticationService> _authMock;
    private readonly Mock<INavigationService> _navMock;
    private readonly RegisterViewModel _viewModel;

    public RegisterViewModelTests()
    {
        _authMock = new Mock<IAuthenticationService>();
        _navMock = new Mock<INavigationService>();
        _viewModel = new RegisterViewModel(_authMock.Object, _navMock.Object);
    }

    [Theory]
    [InlineData("", "Last", "e@e.com", "pass123", "pass123", true, "First name is required")]
    [InlineData("First", "", "e@e.com", "pass123", "pass123", true, "Last name is required")]
    [InlineData("First", "Last", "invalid-email", "pass123", "pass123", true, "Please enter a valid email address")]
    [InlineData("First", "Last", "e@e.com", "123", "123", true, "Password must be at least 6 characters long")]
    [InlineData("First", "Last", "e@e.com", "pass123", "mismatch", true, "Passwords do not match")]
    [InlineData("First", "Last", "e@e.com", "pass123", "pass123", false, "Please accept the terms and conditions")]
    public async Task RegisterCommand_ShouldSetError_WhenValidationFails(
        string fName, string lName, string email, string pass, string confirm, bool terms, string expectedError)
    {
        // Arrange
        _viewModel.FirstName = fName;
        _viewModel.LastName = lName;
        _viewModel.Email = email;
        _viewModel.Password = pass;
        _viewModel.ConfirmPassword = confirm;
        _viewModel.AcceptTerms = terms;

        // Act
        await _viewModel.RegisterCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_viewModel.HasError);
        Assert.Equal(expectedError, _viewModel.ErrorMessage);
        _authMock.Verify(a => a.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCommand_ShouldNavigateBack_OnSuccess()
    {
        // Arrange
        _viewModel.FirstName = "John";
        _viewModel.LastName = "Doe";
        _viewModel.Email = "john@napier.ac.uk";
        _viewModel.Password = "securePass123";
        _viewModel.ConfirmPassword = "securePass123";
        _viewModel.AcceptTerms = true;

        _authMock.Setup(a => a.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(AuthenticationResult.SuccessResult());

        // Act
        await _viewModel.RegisterCommand.ExecuteAsync(null);

        // Assert
        _navMock.Verify(n => n.ShowAlertAsync(It.IsAny<string>(), It.IsAny<string>(), "OK"), Times.Once);
        _navMock.Verify(n => n.NavigateBackAsync(), Times.Once);
        Assert.False(_viewModel.HasError);
    }
}
