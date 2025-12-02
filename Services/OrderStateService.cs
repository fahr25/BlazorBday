using System.Text.Json;
using BlazorBday.Models;

namespace BlazorBday.Services;

public class OrderStateService
{
    private OrderDraft _draft = new();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _isInitialized = false;
    private const string SESSION_KEY = "OrderDraft";

    public event Action? OnChange;

    public OrderStateService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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
    /// Initialize the service by loading state from Session
    /// </summary>
    public Task InitializeAsync()
    {
        if (_isInitialized) return Task.CompletedTask;

        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var json = session?.GetString(SESSION_KEY);

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
            // If Session fails or doesn't exist, start with empty draft
            _draft = new OrderDraft();
        }

        _isInitialized = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Set agency information from step 1
    /// </summary>
    public Task SetAgencyAsync(int agencyId, string agencyName, string threeLetterCode)
    {
        _draft.AgencyId = agencyId;
        _draft.AgencyName = agencyName;
        _draft.ThreeLetterCode = threeLetterCode;

        SaveToSession();
        OnChange?.Invoke();
        
        return Task.CompletedTask; // Return completed task instead of using async
    }

    /// <summary>
    /// Set child demographic information from step 2
    /// </summary>
    public async Task SetChildInfoAsync(DateTime childDateOfBirth)
    {
        // Calculate child age from date of birth
        var today = DateTime.Today;
        var age = today.Year - childDateOfBirth.Year;
        if (childDateOfBirth.Date > today.AddYears(-age)) age--; // Adjust if birthday hasn't occurred this year

        // Validate age range
        if (age < 0 || age > 18)
        {
            throw new ArgumentException("Child age must be between 0 and 18 years.");
        }

        _draft.ChildDateOfBirth = childDateOfBirth;
        _draft.ChildAge = age;

        SaveToSession();
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

        SaveToSession();
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
            SaveToSession();
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
            SaveToSession();
            OnChange?.Invoke();
        }
    }

    /// <summary>
    /// Clear all order data (use after successful checkout)
    /// </summary>
    public async Task ClearAsync()
    {
        _draft = new OrderDraft();
        SaveToSession();
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
    /// Persist current state to Session
    /// </summary>
    private void SaveToSession()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var json = JsonSerializer.Serialize(_draft);
                session.SetString(SESSION_KEY, json);
            }
        }
        catch (Exception)
        {
            // Silently fail if Session is unavailable
        }
    }
}
