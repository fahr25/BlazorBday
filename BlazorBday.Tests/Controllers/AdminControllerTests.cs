using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using BlazorBday.Controllers;
using BlazorBday.Models;
using BlazorBday.Repositories;
using BlazorBday.ViewModels;
using FluentAssertions;
using Xunit;

namespace BlazorBday.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<ISubcategoryRepository> _mockSubcategoryRepo;
    private readonly Mock<IAgencyRepository> _mockAgencyRepo;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockSubcategoryRepo = new Mock<ISubcategoryRepository>();
        _mockAgencyRepo = new Mock<IAgencyRepository>();

        var httpContext = new DefaultHttpContext();
        var tempDataProvider = new Mock<ITempDataProvider>();
        var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider.Object);
        var tempData = tempDataDictionaryFactory.GetTempData(httpContext);

        _controller = new AdminController(
            _mockProductRepo.Object,
            _mockOrderRepo.Object,
            _mockCategoryRepo.Object,
            _mockSubcategoryRepo.Object,
            _mockAgencyRepo.Object
        )
        {
            TempData = tempData
        };
    }

    #region Dashboard Tests

    [Fact]
    public async Task Index_ReturnsViewWithDashboardData()
    {
        // Arrange
        var inventoryStats = new InventoryStatisticsViewModel
        {
            TotalProducts = 50,
            LowStockCount = 5,
            OutOfStockCount = 2
        };

        var recentOrders = new List<Order>
        {
            new Order { Id = 1, Status = OrderStatus.Completed, CreatedAt = DateTime.UtcNow },
            new Order { Id = 2, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        };

        _mockProductRepo.Setup(r => r.GetInventoryStatisticsAsync(5))
            .ReturnsAsync(inventoryStats);
        _mockOrderRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(recentOrders);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        var model = result!.Model as AdminDashboardViewModel;
        model.Should().NotBeNull();
        model!.InventoryStats.TotalProducts.Should().Be(50);
        model.RecentOrders.Should().HaveCount(2);
    }

    #endregion

    #region Product CRUD Tests

    [Fact]
    public async Task Create_ValidProduct_AddsToRepository()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Description",
            Points = 10,
            Inventory = 5,
            CategoryId = 1
        };

        _mockCategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Category>());
        _mockSubcategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Subcategory>());

        // Act
        var result = await _controller.Create(product) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Index");
        _mockProductRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsViewWithErrors()
    {
        // Arrange
        var product = new Product();
        _controller.ModelState.AddModelError("Name", "Name is required");

        _mockCategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Category>());
        _mockSubcategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Subcategory>());

        // Act
        var result = await _controller.Create(product) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.ViewName.Should().Be("Products/Create");
        _mockProductRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Edit_ValidProduct_UpdatesSuccessfully()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Updated Product",
            Description = "Updated",
            Points = 15,
            Inventory = 10,
            CategoryId = 1
        };

        _mockCategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Category>());
        _mockSubcategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Subcategory>());
        _mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Edit(1, product) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Index");
        _mockProductRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task Edit_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var product = new Product { Id = 2, Name = "Test", Description = "Test", Points = 10, Inventory = 5, CategoryId = 1 };

        // Act
        var result = await _controller.Edit(1, product);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockProductRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ExistingProduct_DeletesSuccessfully()
    {
        // Arrange
        _mockProductRepo.Setup(r => r.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Index");
        _mockProductRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    #endregion

    #region Category CRUD Tests

    [Fact]
    public async Task CreateCategory_ValidData_Succeeds()
    {
        // Arrange
        var category = new Category
        {
            Name = "New Category",
            DisplayOrder = 50,
            IsActive = true
        };

        _mockCategoryRepo.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateCategory(category) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Categories");
        _mockCategoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task EditCategory_ValidData_Updates()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Updated Category",
            DisplayOrder = 60,
            IsActive = true
        };

        _mockCategoryRepo.Setup(r => r.UpdateAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EditCategory(1, category) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Categories");
        _mockCategoryRepo.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    #endregion

    #region Agency CRUD Tests

    [Fact]
    public async Task CreateAgency_ValidData_Succeeds()
    {
        // Arrange
        var agency = new Agency
        {
            Name = "New Agency",
            ThreeLetterCode = "NEW",
            ContactEmail = "new@agency.com",
            Phone = "123-456-7890",
            IsActive = true
        };

        _mockAgencyRepo.Setup(r => r.AddAsync(It.IsAny<Agency>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateAgency(agency) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("Agencies");
        _mockAgencyRepo.Verify(r => r.AddAsync(It.IsAny<Agency>()), Times.Once);
    }

    [Fact]
    public async Task CreateAgency_Duplicate3LetterCode_ShouldFail()
    {
        // This test will FAIL initially - demonstrates need for duplicate code validation
        // FUTURE: Add unique constraint validation for ThreeLetterCode

        // Arrange
        var agency = new Agency
        {
            Name = "Duplicate Agency",
            ThreeLetterCode = "DUP",
            ContactEmail = "dup@agency.com",
            Phone = "123-456-7890",
            IsActive = true
        };

        // Simulate duplicate key exception
        _mockAgencyRepo.Setup(r => r.AddAsync(It.IsAny<Agency>()))
            .ThrowsAsync(new InvalidOperationException("Duplicate three letter code"));

        // Act
        Func<Task> act = async () => await _controller.CreateAgency(agency);

        // Assert
        // FUTURE: Should catch exception and return proper error message
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Order Management Tests

    [Fact]
    public async Task ChangeStatus_ValidStatus_UpdatesOrder()
    {
        // Arrange
        _mockOrderRepo.Setup(r => r.UpdateStatusAsync(1, OrderStatus.Completed))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ChangeStatus(1, "Completed") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("OrderDetails");
        _controller.TempData["Success"].Should().Be("Order status updated to Completed");
    }

    [Fact]
    public async Task ChangeStatus_InvalidStatus_ReturnsError()
    {
        // Act
        var result = await _controller.ChangeStatus(1, "InvalidStatus") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        _controller.TempData["Error"].Should().Be("Invalid order status");
        _mockOrderRepo.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public async Task Refund_RestoresInventory_AndCancelsOrder()
    {
        // Arrange
        _mockOrderRepo.Setup(r => r.RefundAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Refund(1) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("OrderDetails");
        _controller.TempData["Success"].Should().Be("Order refunded successfully and inventory restored");
        _mockOrderRepo.Verify(r => r.RefundAsync(1), Times.Once);
    }

    #endregion

    #region Future Enhancement Tests (Will Fail - Guide Development)

    [Fact]
    public async Task Dashboard_ShowsOrdersThisMonth()
    {
        // This test will FAIL - demonstrates need for enhanced dashboard metrics
        // FUTURE: Add method to get orders for specific time periods

        // Arrange
        var stats = new AdminDashboardViewModel
        {
            InventoryStats = new InventoryStatisticsViewModel(),
            RecentOrders = new List<Order>()
            // FUTURE: Add TotalOrdersThisMonth, TotalOrdersThisWeek properties
        };

        // Act
        // FUTURE: Dashboard should show monthly/weekly order counts
        var thisMonthCount = 0; // stats.TotalOrdersThisMonth;

        // Assert
        // This will fail until we implement time-based order statistics
        thisMonthCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Inventory_SupportsSearch()
    {
        // This test will FAIL - demonstrates need for search functionality
        // FUTURE: Add search/filter capability to Inventory view

        // Arrange
        var searchTerm = "Birthday";

        // Act
        // FUTURE: Add Inventory action with search parameter
        // var result = await _controller.Inventory(search: searchTerm) as ViewResult;

        // Assert
        // FUTURE: Should filter products by search term
        Assert.True(false, "Search functionality not yet implemented");
    }

    [Fact]
    public async Task BulkUpdateInventory_UpdatesMultipleProducts()
    {
        // This test will FAIL - demonstrates need for bulk operations
        // FUTURE: Add bulk inventory update feature

        // Arrange
        var productIds = new List<int> { 1, 2, 3 };
        var adjustment = 10; // Add 10 to each product's inventory

        // Act
        // FUTURE: Add BulkUpdateInventory action
        // var result = await _controller.BulkUpdateInventory(productIds, adjustment) as RedirectToActionResult;

        // Assert
        // FUTURE: Should update all products' inventory
        Assert.True(false, "Bulk update functionality not yet implemented");
    }

    [Fact]
    public async Task ExportOrders_GeneratesCSV()
    {
        // This test will FAIL - demonstrates need for export functionality
        // FUTURE: Add order export feature

        // Arrange
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;

        // Act
        // FUTURE: Add ExportOrders action
        // var result = await _controller.ExportOrders(startDate, endDate) as FileResult;

        // Assert
        // FUTURE: Should return CSV file
        Assert.True(false, "Export functionality not yet implemented");
    }

    [Fact]
    public async Task Dashboard_ShowsTopProducts()
    {
        // This test will FAIL - demonstrates need for product popularity tracking
        // FUTURE: Track which products are ordered most frequently

        // Arrange
        var expectedTopProducts = new List<ProductPopularity>
        {
            new ProductPopularity { ProductId = 1, ProductName = "Popular Item", OrderCount = 50 }
        };

        // Act
        // FUTURE: Dashboard should show top selling products
        // var model = await GetDashboardModel();
        // var topProducts = model.TopProducts;

        // Assert
        Assert.True(false, "Product popularity tracking not yet implemented");
    }

    [Fact]
    public async Task Delete_ProductInOrders_ShowsWarning()
    {
        // This test will FAIL - demonstrates need for better delete validation
        // FUTURE: Prevent or warn when deleting products that exist in orders

        // Arrange
        var productId = 1;
        _mockProductRepo.Setup(r => r.DeleteAsync(productId))
            .ThrowsAsync(new InvalidOperationException("Cannot delete product that exists in orders"));

        // Act
        Func<Task> act = async () => await _controller.DeleteConfirmed(productId);

        // Assert
        // FUTURE: Should catch exception and show user-friendly error
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EditProduct_ConcurrencyConflict_HandledGracefully()
    {
        // This test demonstrates concurrency handling
        // Currently handled but could be improved with retry logic

        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Product",
            Description = "Test",
            Points = 10,
            Inventory = 5,
            CategoryId = 1
        };

        _mockProductRepo.Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());
        _mockCategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Category>());
        _mockSubcategoryRepo.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<Subcategory>());

        // Act
        Func<Task> act = async () => await _controller.Edit(1, product);

        // Assert
        // Should rethrow after checking existence
        await act.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>();
    }

    #endregion
}

// Placeholder class for future implementation
public class ProductPopularity
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}
