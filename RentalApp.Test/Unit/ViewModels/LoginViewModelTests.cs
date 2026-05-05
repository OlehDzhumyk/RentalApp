/*
 * @file LoginViewModelTests.cs
 * @brief Unit tests for LoginViewModel using Moq for service abstraction
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Services;
using RentalApp.ViewModels;
using Xunit;

namespace RentalApp.Test.Unit.ViewModels;

/// <summary>
/// Contains unit tests for the LoginViewModel to ensure proper authentication flow
/// and navigation logic.
/// </summary>
public class LoginViewModelTests
{
    private readonly Mock<IAuthenticationService> _authMock;
    private readonly Mock<INavigationService> _navMock;
    private readonly LoginViewModel _viewModel;

    public LoginViewModelTests()
    {
        _authMock = new Mock<IAuthenticationService>();
        _navMock = new Mock<INavigationService>();
        _viewModel = new LoginViewModel(_authMock.Object, _navMock.Object);
    }

    /// <summary>
    /// Verifies that an error message is displayed when the user attempts 
    /// to login with empty credential fields.
    /// </summary>
    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenFieldsAreEmpty()
    {
        // Arrange
        _viewModel.Email = string.Empty;
        _viewModel.Password = string.Empty;

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_viewModel.HasError);
        Assert.Equal("Please enter both email and password", _viewModel.ErrorMessage);
    }

    /// <summary>
    /// Confirms that the application navigates to the root MainPage using 
    /// absolute Shell routing when credentials are valid.
    /// </summary>
    [Fact]
    public async Task LoginCommand_ShouldNavigate_WhenAuthenticationSucceeds()
    {
        // Arrange
        _viewModel.Email = "test@test.com";
        _viewModel.Password = "password";

        var successResult = AuthenticationResult.SuccessResult("Success");

        _authMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(successResult);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        // Updated to match the Shell absolute route implementation
        _navMock.Verify(n => n.NavigateToAsync("//MainPage"), Times.Once);
        Assert.False(_viewModel.HasError);
    }

    /// <summary>
    /// Ensures that an appropriate error message is shown and no navigation 
    /// occurs when authentication fails.
    /// </summary>
    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenAuthenticationFails()
    {
        // Arrange
        _viewModel.Email = "wrong@test.com";
        _viewModel.Password = "wrong";

        var failResult = AuthenticationResult.Failure("Invalid credentials");

        _authMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(failResult);

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_viewModel.HasError);
        Assert.Equal("Invalid credentials", _viewModel.ErrorMessage);
        _navMock.Verify(n => n.NavigateToAsync(It.IsAny<string>()), Times.Never);
    }
}
