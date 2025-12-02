using BlazorBday.Models;
using FluentAssertions;
using Xunit;

namespace BlazorBday.Tests.Models;

public class OrderDraftTests
{
    [Theory]
    [InlineData(0, 65)]
    [InlineData(5, 65)]
    [InlineData(11, 65)]
    public void PointsAssigned_Age0To11_Returns65Points(int age, int expectedPoints)
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = age };

        // Act
        var actualPoints = draft.PointsAssigned;

        // Assert
        actualPoints.Should().Be(expectedPoints);
    }

    [Theory]
    [InlineData(12, 100)]
    [InlineData(15, 100)]
    [InlineData(18, 100)]
    public void PointsAssigned_Age12To18_Returns100Points(int age, int expectedPoints)
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = age };

        // Act
        var actualPoints = draft.PointsAssigned;

        // Assert
        actualPoints.Should().Be(expectedPoints);
    }

    [Fact]
    public void PointsAssigned_AgeBoundary11_Returns65Points()
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = 11 };

        // Act & Assert
        draft.PointsAssigned.Should().Be(65);
    }

    [Fact]
    public void PointsAssigned_AgeBoundary12_Returns100Points()
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = 12 };

        // Act & Assert
        draft.PointsAssigned.Should().Be(100);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(19)]
    [InlineData(25)]
    public void PointsAssigned_InvalidAge_Returns0Points(int age)
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = age };

        // Act & Assert
        draft.PointsAssigned.Should().Be(0);
    }

    [Fact]
    public void PointsUsed_EmptyCart_Returns0()
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = 10 };

        // Act & Assert
        draft.PointsUsed.Should().Be(0);
    }

    [Fact]
    public void PointsUsed_WithItems_CalculatesCorrectSum()
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = 10 };
        draft.Items.Add(new CartItem
        {
            ProductId = 1,
            ProductName = "Card",
            CategoryName = "Cards",
            UnitPoints = 5,
            Quantity = 1
        });
        draft.Items.Add(new CartItem
        {
            ProductId = 2,
            ProductName = "Book",
            CategoryName = "Books",
            UnitPoints = 10,
            Quantity = 1
        });

        // Act
        var pointsUsed = draft.PointsUsed;

        // Assert
        pointsUsed.Should().Be(15); // 5 + 10
    }

    [Fact]
    public void PointsRemaining_CalculatesCorrectly()
    {
        // Arrange
        var draft = new OrderDraft { ChildAge = 10 }; // 65 points
        draft.Items.Add(new CartItem
        {
            ProductId = 1,
            ProductName = "Card",
            CategoryName = "Cards",
            UnitPoints = 5,
            Quantity = 1
        });

        // Act
        var pointsRemaining = draft.PointsRemaining;

        // Assert
        pointsRemaining.Should().Be(60); // 65 - 5
    }

    [Fact]
    public void HasCard_WithCardInCart_ReturnsTrue()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem
        {
            ProductId = 1,
            ProductName = "Birthday Card",
            CategoryName = "Cards",
            UnitPoints = 5,
            Quantity = 1
        });

        // Act & Assert
        draft.HasCard.Should().BeTrue();
    }

    [Fact]
    public void HasCard_WithoutCardInCart_ReturnsFalse()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem
        {
            ProductId = 2,
            ProductName = "Book",
            CategoryName = "Books",
            UnitPoints = 10,
            Quantity = 1
        });

        // Act & Assert
        draft.HasCard.Should().BeFalse();
    }

    [Fact]
    public void HasBook_WithBookInCart_ReturnsTrue()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem
        {
            ProductId = 2,
            ProductName = "Storybook",
            CategoryName = "Books",
            UnitPoints = 10,
            Quantity = 1
        });

        // Act & Assert
        draft.HasBook.Should().BeTrue();
    }

    [Fact]
    public void HasTreat_WithTreatInCart_ReturnsTrue()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem
        {
            ProductId = 3,
            ProductName = "Candy",
            CategoryName = "Treats",
            UnitPoints = 3,
            Quantity = 1
        });

        // Act & Assert
        draft.HasTreat.Should().BeTrue();
    }

    [Fact]
    public void CanSelectGifts_WithAllMandatoryItems_ReturnsTrue()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 2, ProductName = "Book", UnitPoints = 10, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Treats", ProductId = 3, ProductName = "Treat", UnitPoints = 3, Quantity = 1 });

        // Act & Assert
        draft.CanSelectGifts.Should().BeTrue();
    }

    [Fact]
    public void CanSelectGifts_MissingCard_ReturnsFalse()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 2, ProductName = "Book", UnitPoints = 10, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Treats", ProductId = 3, ProductName = "Treat", UnitPoints = 3, Quantity = 1 });

        // Act & Assert
        draft.CanSelectGifts.Should().BeFalse();
    }

    [Fact]
    public void CanSelectGifts_MissingBook_ReturnsFalse()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Treats", ProductId = 3, ProductName = "Treat", UnitPoints = 3, Quantity = 1 });

        // Act & Assert
        draft.CanSelectGifts.Should().BeFalse();
    }

    [Fact]
    public void CanSelectGifts_MissingTreat_ReturnsFalse()
    {
        // Arrange
        var draft = new OrderDraft();
        draft.Items.Add(new CartItem { CategoryName = "Cards", ProductId = 1, ProductName = "Card", UnitPoints = 5, Quantity = 1 });
        draft.Items.Add(new CartItem { CategoryName = "Books", ProductId = 2, ProductName = "Book", UnitPoints = 10, Quantity = 1 });

        // Act & Assert
        draft.CanSelectGifts.Should().BeFalse();
    }

    [Fact]
    public void CartItem_Subtotal_CalculatesCorrectly()
    {
        // Arrange
        var cartItem = new CartItem
        {
            ProductId = 1,
            ProductName = "Test Product",
            CategoryName = "Gifts",
            UnitPoints = 15,
            Quantity = 3
        };

        // Act
        var subtotal = cartItem.Subtotal;

        // Assert
        subtotal.Should().Be(45); // 15 * 3
    }
}
