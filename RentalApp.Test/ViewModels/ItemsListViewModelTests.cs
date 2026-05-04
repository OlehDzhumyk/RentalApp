/*
 * @file ItemsListViewModelTests.cs
 * @brief Unit tests for ItemsListViewModel
 * @author RentalApp Development Team
 * @date 2026
 */

using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Repositories;
using RentalApp.ViewModels;

namespace RentalApp.Test.ViewModels;

public class ItemsListViewModelTests
{
    private readonly Mock<IItemRepository> _itemRepositoryMock;
    private readonly ItemsListViewModel _viewModel;

    public ItemsListViewModelTests()
    {
        _itemRepositoryMock = new Mock<IItemRepository>();

        _viewModel = new ItemsListViewModel(_itemRepositoryMock.Object);
    }

    [Fact]
    public async Task LoadItemsCommand_ShouldPopulateItems_FromRepository()
    {
        // Arrange
        var mockItems = new List<Item>
        {
            new Item { Id = 1, Title = "Drill", IsAvailable = true },
            new Item { Id = 2, Title = "Ladder", IsAvailable = true }
        };
        _itemRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(mockItems);

        // Act
        await _viewModel.LoadItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, _viewModel.Items.Count);
        Assert.Equal("Drill", _viewModel.Items[0].Title);
    }
}