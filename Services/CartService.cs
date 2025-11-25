public class CartService {
    public List<CartItem> Items { get; } = new();
    public event Action OnChange;

    public void AddItem(CartItem item) {
        var existing = Items.FirstOrDefault(i => i.Id == item.Id);
        if (existing != null) existing.Quantity += item.Quantity;
        else Items.Add(item);
        OnChange?.Invoke();
    }

    public void Increment(string id) {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item != null) item.Quantity++;
        OnChange?.Invoke();
    }

    public void Decrement(string id) {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item != null) {
            item.Quantity--;
            if (item.Quantity <= 0) Items.Remove(item);
        }
        OnChange?.Invoke();
    }

    public decimal GetTotal() => Items.Sum(i => i.PointValue * i.Quantity);
}