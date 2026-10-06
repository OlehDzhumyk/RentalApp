/*
 * @file MainViewModelTests.cs
 * @brief Unit tests for the dashboard view model
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Test.Unit.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<IAuthenticationService> _authMock = new();
    private readonly Mock<INavigationService> _navigationMock = new();

    [Fact]
    public void Constructor_ShouldShowSignedInUser()
    {
        _authMock.Setup(a => a.CurrentUser).Returns(new User { FirstName = "Sam", LastName = "Taylor" });
        _authMock.Setup(a => a.HasRole(RoleConstants.Admin)).Returns(false);

        var viewModel = new MainViewModel(_authMock.Object, _navigationMock.Object);

        Assert.Equal("Welcome, Sam Taylor!", viewModel.WelcomeMessage);
        Assert.False(viewModel.IsAdmin);
    }

    [Fact]
    public void LoadUserData_ShouldPickUpANewLogin()
    {
        // Shell keeps the dashboard alive, so the page reloads the user each time it appears
        _authMock.Setup(a => a.CurrentUser).Returns(new User { FirstName = "Sam", LastName = "Taylor" });
        var viewModel = new MainViewModel(_authMock.Object, _navigationMock.Object);

        _authMock.Setup(a => a.CurrentUser).Returns(new User { FirstName = "Morgan", LastName = "Reid" });
        _authMock.Setup(a => a.HasRole(RoleConstants.Admin)).Returns(true);
        viewModel.LoadUserData();

        Assert.Equal("Welcome, Morgan Reid!", viewModel.WelcomeMessage);
        Assert.True(viewModel.IsAdmin);
    }

    [Fact]
    public async Task NavigateToProfile_ShouldOpenProfilePage()
    {
        var viewModel = new MainViewModel(_authMock.Object, _navigationMock.Object);

        await viewModel.NavigateToProfileCommand.ExecuteAsync(null);

        _navigationMock.Verify(n => n.NavigateToAsync("ProfilePage"), Times.Once);
    }

    [Fact]
    public async Task NavigateToUserList_ShouldOpenUserManagement_ForAdmins()
    {
        _authMock.Setup(a => a.HasRole(RoleConstants.Admin)).Returns(true);
        var viewModel = new MainViewModel(_authMock.Object, _navigationMock.Object);

        await viewModel.NavigateToUserListCommand.ExecuteAsync(null);

        _navigationMock.Verify(n => n.NavigateToAsync("UserListPage"), Times.Once);
    }
}
