/*
 * @file UserDetailViewModelTests.cs
 * @brief Unit tests for UserDetailViewModel logic
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Test.ViewModels;

/// <summary>
/// Unit tests for UserDetailViewModel. Verifies correct interaction with 
/// user and role repositories and proper navigation handling.
/// </summary>
public class UserDetailViewModelTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly UserDetailViewModel _viewModel;

    public UserDetailViewModelTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _navigationServiceMock = new Mock<INavigationService>();
        _authServiceMock = new Mock<IAuthenticationService>();

        _viewModel = new UserDetailViewModel(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _navigationServiceMock.Object,
            _authServiceMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        Assert.True(_viewModel.IsActive);
        Assert.Equal("User Details", _viewModel.Title);
        Assert.False(_viewModel.IsBusy);
    }
}