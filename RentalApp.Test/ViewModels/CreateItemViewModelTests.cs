/*
 * @file CreateItemViewModelTests.cs
 * @brief Unit tests for CreateItemViewModel logic
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.Services;
using RentalApp.ViewModels;

namespace RentalApp.Test.ViewModels;

/// <summary>
/// Unit tests for CreateItemViewModel. Verifies interaction between 
/// the UI logic, the repository, and the navigation service.
/// </summary>
public class CreateItemViewModelTests
{
    private readonly Mock<IItemRepository> _itemRepositoryMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly CreateItemViewModel _viewModel;

    public CreateItemViewModelTests()
    {
        _itemRepositoryMock = new Mock<IItemRepository>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _navigationServiceMock = new Mock<INavigationService>();

        // Mock current user to provide an OwnerId for the item
        _authServiceMock.Setup(a => a.CurrentUser).Returns(new User { Id = 1 });

        _viewModel = new CreateItemViewModel(
            _itemRepositoryMock.Object,
            _authServiceMock.Object,
            _navigationServiceMock.Object);
    }

    /// <summary>
    /// Ensures that the SaveCommand calls the repository with correct data and navigates back.
    /// </summary>
    [Fact]
    public async Task SaveCommand_ShouldAddItemAndNavigateBack_WhenDataIsValid()
    {
        // Arrange - Using ItemTitle to match the refactored ViewModel
        _viewModel.ItemTitle = "Test Item";
        _viewModel.Description = "Description";
        _viewModel.PricePerDay = 10.0m;

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _itemRepositoryMock.Verify(r => r.AddAsync(It.Is<Item>(i =>
            i.Title == "Test Item" &&
            i.OwnerId == 1)), Times.Once);

        _navigationServiceMock.Verify(n => n.NavigateBackAsync(), Times.Once);
    }
}