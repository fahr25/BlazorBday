using BlazorBday.Data;
using BlazorBday.Models;
using BlazorBday.Repositories;
using BlazorBday.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BlazorBday.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly MarketShopDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var category = new Category { Name = "Test Category", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product1 = new Product
        {
            Name = "Product 1",
            Description = "Description 1",
            Points = 10,
            Inventory = 5,
            CategoryId = category.Id
        };
        var product2 = new Product
        {
            Name = "Product 2",
            Description = "Description 2",
            Points = 20,
            Inventory = 10,
            CategoryId = category.Id
        };

        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(p => p.Name == "Product 1");
        results.Should().Contain(p => p.Name == "Product 2");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectProduct()
    {
        // Arrange
        var category = new Category { Name = "Test Category", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Points = 15,
            Inventory = 10,
            CategoryId = category.Id
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
        result.Points.Should().Be(15);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsProductToDatabase()
    {
        // Arrange
        var category = new Category { Name = "Test Category", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "New Product",
            Description = "New Description",
            Points = 25,
            Inventory = 8,
            CategoryId = category.Id
        };

        // Act
        await _repository.AddAsync(product);

        // Assert
        var savedProduct = await _context.Products.FindAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("New Product");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingProduct()
    {
        // Arrange
        var category = new Category { Name = "Test Category", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Original Name",
            Description = "Original Description",
            Points = 10,
            Inventory = 5,
            CategoryId = category.Id
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        product.Name = "Updated Name";
        product.Points = 20;
        await _repository.UpdateAsync(product);

        // Assert
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        updatedProduct!.Name.Should().Be("Updated Name");
        updatedProduct.Points.Should().Be(20);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProductFromDatabase()
    {
        // Arrange
        var category = new Category { Name = "Test Category", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Product to Delete",
            Description = "Will be deleted",
            Points = 10,
            Inventory = 5,
            CategoryId = category.Id
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var productId = product.Id;

        // Act
        await _repository.DeleteAsync(productId);

        // Assert
        var deletedProduct = await _context.Products.FindAsync(productId);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ExistingProduct_ReturnsTrue()
    {
        // Arrange
        var category = new Category { Name = "Test Category", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Existing Product",
            Description = "Exists",
            Points = 10,
            Inventory = 5,
            CategoryId = category.Id
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(product.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentProduct_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync(999);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetInventoryStatisticsAsync_CalculatesCorrectly()
    {
        // Arrange
        var category = new Category { Name = "Test Category", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Low stock (2 products)
        _context.Products.Add(new Product
        {
            Name = "Low Stock 1",
            Description = "Low",
            Points = 10,
            Inventory = 2,
            CategoryId = category.Id
        });
        _context.Products.Add(new Product
        {
            Name = "Low Stock 2",
            Description = "Low",
            Points = 10,
            Inventory = 4,
            CategoryId = category.Id
        });

        // Out of stock (1 product)
        _context.Products.Add(new Product
        {
            Name = "Out of Stock",
            Description = "None",
            Points = 10,
            Inventory = 0,
            CategoryId = category.Id
        });

        // In stock (1 product)
        _context.Products.Add(new Product
        {
            Name = "In Stock",
            Description = "Available",
            Points = 10,
            Inventory = 100,
            CategoryId = category.Id
        });

        await _context.SaveChangesAsync();

        // Act
        var stats = await _repository.GetInventoryStatisticsAsync(lowStockThreshold: 5);

        // Assert
        stats.Should().NotBeNull();
        stats.TotalProducts.Should().Be(4);
        stats.OutOfStockCount.Should().Be(1);
        stats.LowStockCount.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
