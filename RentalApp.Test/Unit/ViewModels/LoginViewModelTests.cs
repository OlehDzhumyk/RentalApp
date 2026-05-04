/*
 * @file LoginViewModelTests.cs
 * @brief Unit tests for LoginViewModel using Moq for services
 */

using Moq;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Test.Unit.ViewModels;

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

    [Fact]
    public async Task LoginCommand_ShouldShowError_WhenFieldsAreEmpty()
    {
        // Arrange
        _viewModel.Email = "";
        _viewModel.Password = "";

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_viewModel.HasError); // Виправлено з IsErrorVisible
        Assert.Equal("Please enter both email and password", _viewModel.ErrorMessage);
    }

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
        _navMock.Verify(n => n.NavigateToAsync("MainPage"), Times.Once);
        Assert.False(_viewModel.HasError);
    }

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
