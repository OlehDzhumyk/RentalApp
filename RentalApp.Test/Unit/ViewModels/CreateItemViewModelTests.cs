/*
 * @file CreateItemViewModelTests.cs
 * @brief Unit tests for CreateItemViewModel logic with Location Service support
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
/// the UI logic, the repository, navigation, and location services.
/// </summary>
public class CreateItemViewModelTests
{
    private readonly Mock<IItemRepository> _itemRepositoryMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<ILocationService> _locationServiceMock; // Додано мок
    private readonly CreateItemViewModel _viewModel;

    public CreateItemViewModelTests()
    {
        _itemRepositoryMock = new Mock<IItemRepository>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _locationServiceMock = new Mock<ILocationService>(); // Ініціалізація

        // Mock current user to provide an OwnerId for the item
        _authServiceMock.Setup(a => a.CurrentUser).Returns(new User { Id = 1 });

        // Тепер передаємо всі 4 аргументи
        _viewModel = new CreateItemViewModel(
            _itemRepositoryMock.Object,
            _authServiceMock.Object,
            _navigationServiceMock.Object,
            _locationServiceMock.Object);
    }

    /// <summary>
    /// Ensures that the SaveCommand calls the repository with correct data and navigates back.
    /// </summary>
    [Fact]
    public async Task SaveCommand_ShouldAddItemAndNavigateBack_WhenDataIsValid()
    {
        // Arrange
        _viewModel.ItemTitle = "Test Item";
        _viewModel.Description = "Description";
        _viewModel.PricePerDay = 10.0m;

        // Симулюємо, що GPS повернув координати Едінбурга
        _locationServiceMock.Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _itemRepositoryMock.Verify(r => r.AddAsync(It.Is<Item>(i =>
            i.Title == "Test Item" &&
            i.OwnerId == 1 &&
            i.Location != null)), Times.Once); // Перевіряємо, що локація додана

        _navigationServiceMock.Verify(n => n.NavigateBackAsync(), Times.Once);
    }
}