using BlazorBday.Models;

namespace BlazorBday.Services;

public class CartService {
    public List<CartItem> Items { get; } = new();
    public event Action? OnChange;

    public void AddItem(CartItem item) {
        var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing != null) existing.Quantity += item.Quantity;
        else Items.Add(item);
        OnChange?.Invoke();
    }

    public void Increment(int productId) {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) item.Quantity++;
        OnChange?.Invoke();
    }

    public void Decrement(int productId) {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) {
            item.Quantity--;
            if (item.Quantity <= 0) Items.Remove(item);
        }
        OnChange?.Invoke();
    }

    public int GetTotal() => Items.Sum(i => i.Subtotal);
}