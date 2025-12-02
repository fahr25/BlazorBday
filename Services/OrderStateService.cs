using System.Text.Json;
using BlazorBday.Models;
using Microsoft.JSInterop;

namespace BlazorBday.Services;

public class OrderStateService
{
    private OrderDraft _draft = new();
    private readonly IJSRuntime _js;
    private bool _isInitialized = false;

    public event Action? OnChange;

    public OrderStateService(IJSRuntime js)
    {
        _js = js;
    }

    // Expose OrderDraft properties
    public int? AgencyId => _draft.AgencyId;
    public string AgencyName => _draft.AgencyName;
    public string ThreeLetterCode => _draft.ThreeLetterCode;
    public DateTime? ChildDateOfBirth => _draft.ChildDateOfBirth;
    public int ChildAge => _draft.ChildAge;
    public int PointsAssigned => _draft.PointsAssigned;
    public int PointsUsed => _draft.PointsUsed;
    public int PointsRemaining => _draft.PointsRemaining;
    public bool HasCard => _draft.HasCard;
    public bool HasBook => _draft.HasBook;
    public bool HasTreat => _draft.HasTreat;
    public bool CanSelectGifts => _draft.CanSelectGifts;
    public IReadOnlyList<CartItem> Items => _draft.Items.AsReadOnly();

    /// <summary>
    /// Initialize the service by loading state from localStorage
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            var json = await _js.InvokeAsync<string>("orderStorage.load");
            if (!string.IsNullOrEmpty(json))
            {
                var draft = JsonSerializer.Deserialize<OrderDraft>(json);
                if (draft != null)
                {
                    _draft = draft;
                }
            }
        }
        catch (Exception)
        {
            // If localStorage fails or doesn't exist, start with empty draft
            _draft = new OrderDraft();
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Set agency information from step 1
    /// </summary>
    public async Task SetAgencyAsync(int agencyId, string agencyName, string threeLetterCode)
    {
        _draft.AgencyId = agencyId;
        _draft.AgencyName = agencyName;
        _draft.ThreeLetterCode = threeLetterCode;

        await SaveToStorageAsync();
        OnChange?.Invoke();
    }

    /// <summary>
    /// Set child demographic information from step 2
    /// </summary>
    public async Task SetChildInfoAsync(DateTime childDateOfBirth, int childAge)
    {
        _draft.ChildDateOfBirth = childDateOfBirth;
        _draft.ChildAge = childAge;

        await SaveToStorageAsync();
        OnChange?.Invoke();
    }

    /// <summary>
    /// Add an item to the cart
    /// </summary>
    public async Task AddItemAsync(CartItem item)
    {
        var existing = _draft.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            _draft.Items.Add(item);
        }

        await SaveToStorageAsync();
        OnChange?.Invoke();
    }

    /// <summary>
    /// Remove an item from the cart
    /// </summary>
    public async Task RemoveItemAsync(int productId)
    {
        var item = _draft.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _draft.Items.Remove(item);
            await SaveToStorageAsync();
            OnChange?.Invoke();
        }
    }

    /// <summary>
    /// Remove all items from a specific category (for replacing card/book/treat)
    /// </summary>
    public async Task RemoveCategoryItemsAsync(string categoryName)
    {
        var itemsToRemove = _draft.Items.Where(i => i.CategoryName == categoryName).ToList();
        foreach (var item in itemsToRemove)
        {
            _draft.Items.Remove(item);
        }

        if (itemsToRemove.Any())
        {
            await SaveToStorageAsync();
            OnChange?.Invoke();
        }
    }

    /// <summary>
    /// Clear all order data (use after successful checkout)
    /// </summary>
    public async Task ClearAsync()
    {
        _draft = new OrderDraft();
        await SaveToStorageAsync();
        OnChange?.Invoke();
    }

    /// <summary>
    /// Get the complete OrderDraft object (for API calls to ShopController)
    /// </summary>
    public OrderDraft GetDraft()
    {
        return _draft;
    }

    /// <summary>
    /// Persist current state to browser localStorage
    /// </summary>
    private async Task SaveToStorageAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_draft);
            await _js.InvokeVoidAsync("orderStorage.save", json);
        }
        catch (Exception)
        {
            // Silently fail if localStorage is unavailable
        }
    }
}
