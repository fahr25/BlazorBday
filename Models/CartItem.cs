namespace BlazorBday.Models;



// lightweight session DTO for the shop flow
public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty; // Track category for validation
    public int UnitPoints { get; set; }
    public int Quantity { get; set; }
    public int Subtotal => UnitPoints * Quantity;
}
