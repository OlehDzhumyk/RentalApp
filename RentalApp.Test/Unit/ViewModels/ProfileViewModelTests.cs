/*
 * @file ProfileViewModelTests.cs
 * @brief Unit tests for the profile view model
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Test.Unit.ViewModels;

public class ProfileViewModelTests
{
    private readonly Mock<IAuthenticationService> _authMock = new();
    private readonly ProfileViewModel _viewModel;

    public ProfileViewModelTests()
    {
        _authMock.Setup(a => a.CurrentUser).Returns(new User { FirstName = "Sam", LastName = "Taylor", Email = "sam@example.com" });
        _authMock.Setup(a => a.CurrentUserRoles).Returns(new List<string> { "Admin", "User" });
        _viewModel = new ProfileViewModel(_authMock.Object, Mock.Of<INavigationService>());
    }

    [Fact]
    public void Constructor_ShouldShowUserAndRoles()
    {
        Assert.Equal("sam@example.com", _viewModel.CurrentUser?.Email);
        Assert.Equal("Roles: Admin, User", _viewModel.RolesDisplay);
    }

    [Theory]
    [InlineData("", "newpass1", "newpass1", "Current password is required")]
    [InlineData("oldpass1", "short", "short", "at least 6 characters")]
    [InlineData("oldpass1", "newpass1", "different", "do not match")]
    [InlineData("oldpass1", "oldpass1", "oldpass1", "must be different")]
    public async Task ChangePassword_ShouldValidateInput(string current, string newPassword, string confirm, string expectedError)
    {
        _viewModel.CurrentPassword = current;
        _viewModel.NewPassword = newPassword;
        _viewModel.ConfirmNewPassword = confirm;

        await _viewModel.ChangePasswordCommand.ExecuteAsync(null);

        Assert.Contains(expectedError, _viewModel.ErrorMessage);
        _authMock.Verify(a => a.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_ShouldShowError_WhenCurrentPasswordIsWrong()
    {
        _authMock.Setup(a => a.ChangePasswordAsync("oldpass1", "newpass1")).ReturnsAsync(false);
        _viewModel.CurrentPassword = "oldpass1";
        _viewModel.NewPassword = "newpass1";
        _viewModel.ConfirmNewPassword = "newpass1";

        await _viewModel.ChangePasswordCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasError);
        Assert.Contains("check your current password", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_ShouldClearFields_OnSuccess()
    {
        _authMock.Setup(a => a.ChangePasswordAsync("oldpass1", "newpass1")).ReturnsAsync(true);
        _viewModel.IsChangingPassword = true;
        _viewModel.CurrentPassword = "oldpass1";
        _viewModel.NewPassword = "newpass1";
        _viewModel.ConfirmNewPassword = "newpass1";

        await _viewModel.ChangePasswordCommand.ExecuteAsync(null);

        Assert.False(_viewModel.HasError);
        Assert.False(_viewModel.IsChangingPassword);
        Assert.Equal(string.Empty, _viewModel.NewPassword);
    }
}
