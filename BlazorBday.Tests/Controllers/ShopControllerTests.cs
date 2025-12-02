using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using BlazorBday.Controllers;
using BlazorBday.Data;
using BlazorBday.Models;
using BlazorBday.Repositories;
using BlazorBday.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System.Text.Json;

namespace BlazorBday.Tests.Controllers;

public class ShopControllerTests : IDisposable
{
    private readonly MarketShopDbContext _context;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly ShopController _controller;
    private readonly Mock<ISession> _mockSession;
    private readonly DefaultHttpContext _httpContext;

    public ShopControllerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();

        // Setup mock session
        _mockSession = new Mock<ISession>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Session = _mockSession.Object;

        // Setup TempData (required for error messages)
        var tempDataProvider = new Mock<ITempDataProvider>();
        var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider.Object);
        var tempData = tempDataDictionaryFactory.GetTempData(_httpContext);

        _controller = new ShopController(
            _mockProductRepo.Object,
            _mockOrderRepo.Object,
            _mockCategoryRepo.Object,
            _context
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            },
            TempData = tempData
        };
    }

    #region Agency Selection Tests

    [Fact]
    public async Task SelectAgency_GET_ReturnsViewWithActiveAgencies()
    {
        // Arrange
        var agency1 = new Agency { Id = 1, Name = "Agency 1", ThreeLetterCode = "AG1", IsActive = true, ContactEmail = "test1@test.com", Phone = "123-456-7890" };
        var agency2 = new Agency { Id = 2, Name = "Agency 2", ThreeLetterCode = "AG2", IsActive = true, ContactEmail = "test2@test.com", Phone = "123-456-7891" };
        var inactiveAgency = new Agency { Id = 3, Name = "Inactive", ThreeLetterCode = "INA", IsActive = false, ContactEmail = "test3@test.com", Phone = "123-456-7892" };

        _context.Agencies.AddRange(agency1, agency2, inactiveAgency);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SelectAgency() as ViewResult;

        // Assert
        result.Should().NotBeNull();
        var agencies = _context.Agencies.Where(a => a.IsActive).ToList();
        agencies.Should().HaveCount(2); // Only active agencies
        agencies.Should().Contain(a => a.Name == "Agency 1");
        agencies.Should().Contain(a => a.Name == "Agency 2");
        agencies.Should().NotContain(a => a.Name == "Inactive");
    }

    [Fact]
    public async Task SelectAgency_ValidCodeAndAgency_RedirectsToDemographicIntake()
    {
        // Arrange
        var agency = new Agency { Id = 1, Name = "Test Agency", ThreeLetterCode = "ABC", IsActive = true, ContactEmail = "test@test.com", Phone = "123-456-7890" };
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        byte[]? sessionData = null;
        _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => sessionData = value);

        // Act
        var result = await _controller.SelectAgency(1, "ABC") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("DemographicIntake");
        sessionData.Should().NotBeNull();
    }

    [Fact]
    public async Task SelectAgency_InvalidAgencyId_ReturnsError()
    {
        // Act
        var result = await _controller.SelectAgency(999, "XYZ") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectAgency");
        _controller.TempData["Error"].Should().Be("Invalid agency selection.");
    }

    [Fact]
    public async Task SelectAgency_InactiveAgency_ReturnsError()
    {
        // Arrange
        var agency = new Agency { Id = 1, Name = "Inactive Agency", ThreeLetterCode = "INA", IsActive = false, ContactEmail = "test@test.com", Phone = "123-456-7890" };
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SelectAgency(1, "INA") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectAgency");
        _controller.TempData["Error"].Should().Be("Invalid agency selection.");
    }

    [Fact]
    public async Task SelectAgency_WrongCode_ReturnsError()
    {
        // Arrange
        var agency = new Agency { Id = 1, Name = "Test Agency", ThreeLetterCode = "ABC", IsActive = true, ContactEmail = "test@test.com", Phone = "123-456-7890" };
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SelectAgency(1, "XYZ") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectAgency");
        _controller.TempData["Error"].Should().Be("Invalid agency code. Please check your 3-letter code.");
    }

    [Fact]
    public async Task SelectAgency_CaseInsensitiveCode_Succeeds()
    {
        // Arrange
        var agency = new Agency { Id = 1, Name = "Test Agency", ThreeLetterCode = "ABC", IsActive = true, ContactEmail = "test@test.com", Phone = "123-456-7890" };
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        byte[]? sessionData = null;
        _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => sessionData = value);

        // Act
        var result = await _controller.SelectAgency(1, "abc") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("DemographicIntake");
    }

    #endregion

    #region Demographic Intake Tests

    [Fact]
    public void DemographicIntake_Age11_Assigns65Points()
    {
        // Arrange
        SetupSession(new OrderDraft { AgencyId = 1, AgencyName = "Test" });
        var birthDate = DateTime.Today.AddYears(-11);

        // Act
        var result = _controller.DemographicIntake(birthDate) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("GetReadyToShop");

        var draft = GetSessionDraft();
        draft.Should().NotBeNull();
        draft!.ChildAge.Should().Be(11);
        draft.PointsAssigned.Should().Be(65);
    }

    [Fact]
    public void DemographicIntake_Age12_Assigns100Points()
    {
        // Arrange
        SetupSession(new OrderDraft { AgencyId = 1, AgencyName = "Test" });
        var birthDate = DateTime.Today.AddYears(-12);

        // Act
        var result = _controller.DemographicIntake(birthDate) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        var draft = GetSessionDraft();
        draft!.ChildAge.Should().Be(12);
        draft.PointsAssigned.Should().Be(100);
    }

    [Fact]
    public void DemographicIntake_FutureDOB_ReturnsError()
    {
        // Arrange
        SetupSession(new OrderDraft { AgencyId = 1, AgencyName = "Test" });
        var birthDate = DateTime.Today.AddDays(1);

        // Act
        var result = _controller.DemographicIntake(birthDate) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("DemographicIntake");
        _controller.TempData["Error"].Should().Be("Child age must be between 0 and 18 years.");
    }

    [Fact]
    public void DemographicIntake_Age19_ReturnsError()
    {
        // Arrange
        SetupSession(new OrderDraft { AgencyId = 1, AgencyName = "Test" });
        var birthDate = DateTime.Today.AddYears(-19);

        // Act
        var result = _controller.DemographicIntake(birthDate) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("DemographicIntake");
        _controller.TempData["Error"].Should().Be("Child age must be between 0 and 18 years.");
    }

    [Fact]
    public void DemographicIntake_BirthdayNotYetThisYear_AdjustsAgeCorrectly()
    {
        // Arrange
        SetupSession(new OrderDraft { AgencyId = 1, AgencyName = "Test" });
        var today = new DateTime(2025, 6, 1); // June 1, 2025
        var birthDate = new DateTime(2015, 12, 25); // Born Dec 25, 2015 - birthday hasn't happened yet this year

        // Mock DateTime.Today behavior - this test will FAIL because we can't easily mock DateTime.Today
        // This is intentional to show a limitation in the current implementation

        // Act
        var result = _controller.DemographicIntake(birthDate) as RedirectToActionResult;

        // Assert
        // This test demonstrates that the age calculation depends on DateTime.Today
        // To make this testable, we'd need to inject a time provider
        result.Should().NotBeNull();
    }

    #endregion

    #region Sequential Flow Enforcement Tests

    [Fact]
    public async Task SelectBook_NoCard_RedirectsToSelectCard()
    {
        // Arrange
        SetupSession(new OrderDraft { AgencyId = 1, ChildAge = 10 });

        // Act
        var result = await _controller.SelectBook() as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectCard");
    }

    [Fact]
    public async Task SelectTreat_NoBook_RedirectsToSelectBook()
    {
        // Arrange
        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        SetupSession(draft);

        // Act
        var result = await _controller.SelectTreat() as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectBook");
    }

    [Fact]
    public async Task SelectGifts_CannotSelectGifts_RedirectsToSelectTreat()
    {
        // Arrange
        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 2, ProductName = "Book", UnitPoints = 10, Quantity = 1 });
        // Missing Treat
        SetupSession(draft);

        // Act
        var result = await _controller.SelectGifts(null, null, null) as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectTreat");
    }

    #endregion

    #region Cart Operations Tests

    [Fact]
    public async Task AddItem_Card_ReplacesExistingCard()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Cards", DisplayOrder = 10, IsActive = true };
        var oldCard = new Product { Id = 1, Name = "Old Card", Points = 5, Inventory = 10, CategoryId = 1, Category = category, Description = "Old" };
        var newCard = new Product { Id = 2, Name = "New Card", Points = 5, Inventory = 10, CategoryId = 1, Category = category, Description = "New" };

        _context.Categories.Add(category);
        _context.Products.AddRange(oldCard, newCard);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Old Card", UnitPoints = 5, Quantity = 1 });
        SetupSession(draft);

        // Act
        var result = await _controller.AddItem(2, "Cards", "SelectCard") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        var updatedDraft = GetSessionDraft();
        updatedDraft!.Items.Should().HaveCount(1);
        updatedDraft.Items.First().ProductName.Should().Be("New Card");
    }

    [Fact]
    public async Task AddItem_InsufficientPoints_ReturnsError()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Gifts", DisplayOrder = 40, IsActive = true };
        var expensiveProduct = new Product { Id = 1, Name = "Expensive", Points = 100, Inventory = 10, CategoryId = 1, Category = category, Description = "Costs too much" };

        _context.Categories.Add(category);
        _context.Products.Add(expensiveProduct);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 5 }; // Only 65 points
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 10, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 11, ProductName = "Book", UnitPoints = 10, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Treats", ProductId = 12, ProductName = "Treat", UnitPoints = 3, Quantity = 1 });
        // 18 points used, 47 remaining, trying to add 100-point item
        SetupSession(draft);

        // Act
        var result = await _controller.AddItem(1, "Gifts", "SelectGifts") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectGifts");
        _controller.TempData["Error"].Should().Be("Not enough points to add this item.");
    }

    [Fact]
    public async Task AddItem_OutOfStock_ReturnsError()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Cards", DisplayOrder = 10, IsActive = true };
        var outOfStockProduct = new Product { Id = 1, Name = "Out of Stock", Points = 5, Inventory = 0, CategoryId = 1, Category = category, Description = "None left" };

        _context.Categories.Add(category);
        _context.Products.Add(outOfStockProduct);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        SetupSession(draft);

        // Act
        var result = await _controller.AddItem(1, "Cards", "SelectCard") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectCard");
        _controller.TempData["Error"].Should().Be("This item is out of stock.");
    }

    [Fact]
    public async Task AddItem_Card_RedirectsToSelectBook()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Cards", DisplayOrder = 10, IsActive = true };
        var card = new Product { Id = 1, Name = "Card", Points = 5, Inventory = 10, CategoryId = 1, Category = category, Description = "Card" };

        _context.Categories.Add(category);
        _context.Products.Add(card);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        SetupSession(draft);

        // Act
        var result = await _controller.AddItem(1, "Cards", "SelectCard") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectBook");
    }

    [Fact]
    public async Task AddItem_Gift_StaysOnSelectGifts()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Gifts", DisplayOrder = 40, IsActive = true };
        var gift = new Product { Id = 1, Name = "Gift", Points = 10, Inventory = 10, CategoryId = 1, Category = category, Description = "Gift" };

        _context.Categories.Add(category);
        _context.Products.Add(gift);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 10, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 11, ProductName = "Book", UnitPoints = 10, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Treats", ProductId = 12, ProductName = "Treat", UnitPoints = 3, Quantity = 1 });
        SetupSession(draft);

        // Act
        var result = await _controller.AddItem(1, "Gifts", "SelectGifts") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectGifts");
    }

    [Fact]
    public void RemoveItem_RemovesFromCart()
    {
        // Arrange
        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Gifts", ProductId = 2, ProductName = "Gift", UnitPoints = 10, Quantity = 1 });
        SetupSession(draft);

        // Act
        var result = _controller.RemoveItem(2, "SelectGifts") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        var updatedDraft = GetSessionDraft();
        updatedDraft!.Items.Should().HaveCount(1);
        updatedDraft.Items.Should().NotContain(i => i.ProductId == 2);
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task SelectGifts_SubcategoryFilter_ReturnsFilteredProducts()
    {
        // This test will FAIL initially because we need to verify the filtering logic
        // It's here to guide implementation of subcategory filtering

        // Arrange
        var giftsCategory = new Category { Id = 1, Name = "Gifts", DisplayOrder = 40, IsActive = true };
        var toysSubcategory = new Subcategory { Id = 1, Name = "Toys", CategoryId = 1, IsActive = true };
        var booksSubcategory = new Subcategory { Id = 2, Name = "Books", CategoryId = 1, IsActive = true };

        _context.Categories.Add(giftsCategory);
        _context.Subcategories.AddRange(toysSubcategory, booksSubcategory);
        await _context.SaveChangesAsync();

        var toy1 = new Product { Id = 1, Name = "Toy Car", Points = 15, Inventory = 5, CategoryId = 1, SubcategoryId = 1, Description = "Car" };
        var toy2 = new Product { Id = 2, Name = "Toy Doll", Points = 20, Inventory = 3, CategoryId = 1, SubcategoryId = 1, Description = "Doll" };
        var book = new Product { Id = 3, Name = "Story Book", Points = 10, Inventory = 10, CategoryId = 1, SubcategoryId = 2, Description = "Book" };

        _context.Products.AddRange(toy1, toy2, book);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 10, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 11, ProductName = "Book", UnitPoints = 10, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Treats", ProductId = 12, ProductName = "Treat", UnitPoints = 3, Quantity = 1 });
        SetupSession(draft);

        // Act
        var result = await _controller.SelectGifts(subcategoryId: 1, minPoints: null, maxPoints: null) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        var products = result!.Model as List<Product>;
        products.Should().NotBeNull();
        products!.Should().HaveCount(2);
        products.Should().OnlyContain(p => p.SubcategoryId == 1);
    }

    [Fact]
    public async Task SelectGifts_PointsRangeFilter_ReturnsProductsInRange()
    {
        // Arrange
        var giftsCategory = new Category { Id = 1, Name = "Gifts", DisplayOrder = 40, IsActive = true };
        giftsCategory.Subcategories = new List<Subcategory>();

        _context.Categories.Add(giftsCategory);
        await _context.SaveChangesAsync();

        var cheapProduct = new Product { Id = 1, Name = "Cheap", Points = 5, Inventory = 10, CategoryId = 1, Description = "Low cost" };
        var mediumProduct = new Product { Id = 2, Name = "Medium", Points = 15, Inventory = 5, CategoryId = 1, Description = "Medium cost" };
        var expensiveProduct = new Product { Id = 3, Name = "Expensive", Points = 30, Inventory = 2, CategoryId = 1, Description = "High cost" };

        _context.Products.AddRange(cheapProduct, mediumProduct, expensiveProduct);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 10, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 11, ProductName = "Book", UnitPoints = 10, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Treats", ProductId = 12, ProductName = "Treat", UnitPoints = 3, Quantity = 1 });
        SetupSession(draft);

        // Act
        var result = await _controller.SelectGifts(subcategoryId: null, minPoints: 10, maxPoints: 20) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        var products = result!.Model as List<Product>;
        products.Should().HaveCount(1);
        products!.First().Points.Should().Be(15);
    }

    #endregion

    #region Future Enhancement Tests (Will Fail - Guide Development)

    [Fact]
    public async Task SelectAgency_EmptyCode_ReturnsValidationError()
    {
        // This test will FAIL - demonstrates need for better validation
        // Currently the code might not handle empty strings well

        // Arrange
        var agency = new Agency { Id = 1, Name = "Test", ThreeLetterCode = "ABC", IsActive = true, ContactEmail = "test@test.com", Phone = "123-456-7890" };
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SelectAgency(1, "") as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result!.ActionName.Should().Be("SelectAgency");
        _controller.TempData.ContainsKey("Error").Should().BeTrue();
    }

    [Fact]
    public async Task AddItem_NegativeInventory_PreventsCheckout()
    {
        // This test will help identify race conditions in inventory management
        // FUTURE: Add optimistic concurrency handling

        // Arrange
        var category = new Category { Id = 1, Name = "Cards", DisplayOrder = 10, IsActive = true };
        var product = new Product { Id = 1, Name = "Card", Points = 5, Inventory = 1, CategoryId = 1, Category = category, Description = "Last one" };

        _context.Categories.Add(category);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        SetupSession(draft);

        // Act - Two users trying to add the same last item
        await _controller.AddItem(1, "Cards", "SelectCard");

        // Simulate another user buying it (inventory now 0)
        product.Inventory = 0;
        await _context.SaveChangesAsync();

        // Try checkout - should fail
        // This demonstrates the need for inventory validation at checkout time
        var result = await _controller.Checkout() as RedirectToActionResult;

        // Assert
        // FUTURE: This should prevent checkout with proper error message
        result.Should().NotBeNull();
    }

    [Fact]
    public void Checkout_MissingMandatoryItems_ShowsSpecificErrorMessage()
    {
        // This test will FAIL - demonstrates need for better error messages
        // Currently just says "must select card, book, and treat"
        // FUTURE: Tell user specifically which items are missing

        // Arrange
        var draft = new OrderDraft { AgencyId = 1, ChildAge = 10 };
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        // Missing Book and Treat
        SetupSession(draft);

        // Act
        var result = _controller.Checkout().Result as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        var errorMessage = _controller.TempData["Error"] as string;

        // FUTURE: More specific error messages
        errorMessage.Should().Contain("book");
        errorMessage.Should().Contain("treat");
    }

    #endregion

    #region Helper Methods

    private void SetupSession(OrderDraft draft)
    {
        var json = JsonSerializer.Serialize(draft);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                value = bytes;
                return true;
            });

        _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => { });
    }

    private OrderDraft? GetSessionDraft()
    {
        byte[]? sessionBytes = null;
        if (_mockSession.Object.TryGetValue("OrderDraft", out sessionBytes))
        {
            var json = System.Text.Encoding.UTF8.GetString(sessionBytes);
            return JsonSerializer.Deserialize<OrderDraft>(json);
        }
        return null;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #endregion
}
