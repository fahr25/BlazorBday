using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorBday.Models;
using BlazorBday.Repositories;
using BlazorBday.Data;
using System.Text.Json;

namespace BlazorBday.Controllers;

public class ShopController : Controller
{
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly ICategoryRepository _categories;
    private readonly MarketShopDbContext _db;
    private const string SESSION_KEY = "OrderDraft";

    public ShopController(
        IProductRepository products,
        IOrderRepository orders,
        ICategoryRepository categories,
        MarketShopDbContext db)
    {
        _products = products;
        _orders = orders;
        _categories = categories;
        _db = db;
    }

    // STEP 1: Agency Selection
    // GET: /Shop/SelectAgency
    public async Task<IActionResult> SelectAgency()
    {
        var agencies = await _db.Agencies.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync();
        var draft = GetDraft() ?? new OrderDraft();
        ViewBag.Agencies = agencies;
        return View(draft);
    }

    // POST: /Shop/SelectAgency
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectAgency(int agencyId, string threeLetterCode)
    {
        var agency = await _db.Agencies.FindAsync(agencyId);

        // Validate agency exists, is active, and code matches
        if (agency == null || !agency.IsActive)
        {
            TempData["Error"] = "Invalid agency selection.";
            return RedirectToAction("SelectAgency");
        }

        if (!string.Equals(agency.ThreeLetterCode, threeLetterCode, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Invalid agency code. Please check your 3-letter code.";
            return RedirectToAction("SelectAgency");
        }

        // Store agency info in draft
        var draft = GetDraft() ?? new OrderDraft();
        draft.AgencyId = agency.Id;
        draft.AgencyName = agency.Name;
        draft.ThreeLetterCode = agency.ThreeLetterCode;
        SetDraft(draft);

        return RedirectToAction("DemographicIntake");
    }

    // STEP 2: Demographic Intake
    // GET: /Shop/DemographicIntake
    public IActionResult DemographicIntake()
    {
        var draft = GetDraft();
        if (draft?.AgencyId == null) return RedirectToAction("SelectAgency");
        return View(draft);
    }

    // POST: /Shop/DemographicIntake
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DemographicIntake(DateTime childDateOfBirth)
    {
        var draft = GetDraft();
        if (draft?.AgencyId == null) return RedirectToAction("SelectAgency");

        // Calculate child age from date of birth
        var today = DateTime.Today;
        var age = today.Year - childDateOfBirth.Year;
        if (childDateOfBirth.Date > today.AddYears(-age)) age--; // Adjust if birthday hasn't occurred this year

        // Validate age range
        if (age < 0 || age > 18)
        {
            TempData["Error"] = "Child age must be between 0 and 18 years.";
            return RedirectToAction("DemographicIntake");
        }

        draft.ChildDateOfBirth = childDateOfBirth;
        draft.ChildAge = age;
        SetDraft(draft);

        return RedirectToAction("GetReadyToShop");
    }

    // STEP 3: Get Ready to Shop
    // GET: /Shop/GetReadyToShop
    public IActionResult GetReadyToShop()
    {
        var draft = GetDraft();
        if (draft?.AgencyId == null) return RedirectToAction("SelectAgency");
        if (draft.ChildAge == 0 && draft.ChildDateOfBirth == null) return RedirectToAction("DemographicIntake");
        return View(draft);
    }

    // STEP 4: Card Selection (must select exactly 1)
    // GET: /Shop/SelectCard
    public async Task<IActionResult> SelectCard()
    {
        var draft = GetDraft();
        if (draft?.AgencyId == null) return RedirectToAction("SelectAgency");

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Cards" && c.IsActive);
        if (category == null)
        {
            TempData["Error"] = "Cards category not found. Please contact administrator.";
            return RedirectToAction("GetReadyToShop");
        }

        var products = await _db.Products
            .Where(p => p.CategoryId == category.Id && p.Inventory > 0)
            .ToListAsync();

        ViewData["CategoryName"] = "Cards";
        ViewData["StepNumber"] = 4;
        ViewData["StepTitle"] = "Select a Card";
        return View("SelectProduct", products);
    }

    // STEP 5: Book Selection (must select exactly 1)
    // GET: /Shop/SelectBook
    public async Task<IActionResult> SelectBook()
    {
        var draft = GetDraft();
        if (!draft.HasCard) return RedirectToAction("SelectCard");

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Books" && c.IsActive);
        if (category == null)
        {
            TempData["Error"] = "Books category not found. Please contact administrator.";
            return RedirectToAction("SelectCard");
        }

        var products = await _db.Products
            .Where(p => p.CategoryId == category.Id && p.Inventory > 0)
            .ToListAsync();

        ViewData["CategoryName"] = "Books";
        ViewData["StepNumber"] = 5;
        ViewData["StepTitle"] = "Select a Book";
        return View("SelectProduct", products);
    }

    // STEP 6: Treat Selection (must select exactly 1)
    // GET: /Shop/SelectTreat
    public async Task<IActionResult> SelectTreat()
    {
        var draft = GetDraft();
        if (!draft.HasBook) return RedirectToAction("SelectBook");

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Treats" && c.IsActive);
        if (category == null)
        {
            TempData["Error"] = "Treats category not found. Please contact administrator.";
            return RedirectToAction("SelectBook");
        }

        var products = await _db.Products
            .Where(p => p.CategoryId == category.Id && p.Inventory > 0)
            .ToListAsync();

        ViewData["CategoryName"] = "Treats";
        ViewData["StepNumber"] = 6;
        ViewData["StepTitle"] = "Select a Sweet Treat";
        return View("SelectProduct", products);
    }

    // STEP 7: Gift Selection (multiple items with filters)
    // GET: /Shop/SelectGifts
    public async Task<IActionResult> SelectGifts(int? subcategoryId, int? minPoints, int? maxPoints)
    {
        var draft = GetDraft();
        if (!draft.CanSelectGifts) return RedirectToAction("SelectTreat");

        var category = await _db.Categories
            .Include(c => c.Subcategories)
            .FirstOrDefaultAsync(c => c.Name == "Gifts" && c.IsActive);

        if (category == null)
        {
            TempData["Error"] = "Gifts category not found. Please contact administrator.";
            return RedirectToAction("SelectTreat");
        }

        // Build query with filters
        var query = _db.Products
            .Where(p => p.CategoryId == category.Id && p.Inventory > 0);

        if (subcategoryId.HasValue)
            query = query.Where(p => p.SubcategoryId == subcategoryId.Value);

        if (minPoints.HasValue)
            query = query.Where(p => p.Points >= minPoints.Value);

        if (maxPoints.HasValue)
            query = query.Where(p => p.Points <= maxPoints.Value);

        var products = await query.OrderBy(p => p.Points).ToListAsync();

        ViewData["CategoryName"] = "Gifts";
        ViewData["StepNumber"] = 7;
        ViewData["StepTitle"] = "Select Gifts";
        ViewData["Subcategories"] = category.Subcategories.Where(s => s.IsActive).ToList();
        ViewData["SelectedSubcategoryId"] = subcategoryId;
        ViewData["MinPoints"] = minPoints;
        ViewData["MaxPoints"] = maxPoints;
        ViewData["PointsRemaining"] = draft.PointsRemaining;

        return View(products);
    }

    // POST: /Shop/AddItem - Universal add to cart
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int productId, string categoryName, string returnAction)
    {
        var p = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == productId);
        var draft = GetDraft() ?? new OrderDraft();

        if (p == null) return NotFound();
        if (p.Inventory < 1)
        {
            TempData["Error"] = "This item is out of stock.";
            return RedirectToAction(returnAction);
        }

        // For Cards, Books, Treats: enforce exactly 1 item (replace if exists)
        if (categoryName == "Cards" || categoryName == "Books" || categoryName == "Treats")
        {
            var existing = draft.Items.FirstOrDefault(i => i.CategoryName == categoryName);
            if (existing != null)
            {
                draft.Items.Remove(existing); // Replace existing selection
            }
        }

        // Check points
        if (draft.PointsRemaining < p.Points)
        {
            TempData["Error"] = "Not enough points to add this item.";
            return RedirectToAction(returnAction);
        }

        // Add new item
        draft.Items.Add(new CartItem
        {
            ProductId = p.Id,
            ProductName = p.Name,
            CategoryName = p.Category.Name,
            UnitPoints = p.Points,
            Quantity = 1
        });

        SetDraft(draft);

        // Auto-advance for mandatory single selections
        if (categoryName == "Cards") return RedirectToAction("SelectBook");
        if (categoryName == "Books") return RedirectToAction("SelectTreat");
        if (categoryName == "Treats") return RedirectToAction("SelectGifts");

        // For gifts, stay on same page
        return RedirectToAction(returnAction);
    }

    // POST: /Shop/RemoveItem - Remove from cart
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int productId, string returnAction)
    {
        var draft = GetDraft();
        if (draft == null) return RedirectToAction("SelectAgency");

        var item = draft.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) draft.Items.Remove(item);

        SetDraft(draft);
        return RedirectToAction(returnAction);
    }

    // STEP 8: Review Cart
    // GET: /Shop/Review
    public IActionResult Review()
    {
        var draft = GetDraft();
        if (draft?.AgencyId == null) return RedirectToAction("SelectAgency");
        if (!draft.CanSelectGifts) return RedirectToAction("SelectCard");

        return View(draft);
    }

    // POST: /Shop/Checkout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var draft = GetDraft();
        if (draft == null || !draft.Items.Any()) return RedirectToAction("SelectAgency");
        if (!draft.CanSelectGifts)
        {
            TempData["Error"] = "You must select a card, book, and treat before checkout.";
            return RedirectToAction("Review");
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                AgencyId = draft.AgencyId ?? 0,
                ChildDateOfBirth = draft.ChildDateOfBirth ?? DateTime.MinValue,
                ChildAge = draft.ChildAge,
                PointsAssigned = draft.PointsAssigned,
                PointsUsed = draft.PointsUsed,
                TotalItems = draft.Items.Sum(i => i.Quantity),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            foreach (var ci in draft.Items)
            {
                var p = await _db.Products.FindAsync(ci.ProductId);
                if (p == null) throw new Exception("Product not found during checkout.");
                if (p.Inventory < ci.Quantity) throw new Exception($"Insufficient inventory for {p.Name}.");

                // Decrement inventory
                p.Inventory -= ci.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.ProductName,
                    UnitPoints = ci.UnitPoints,
                    Quantity = ci.Quantity,
                    SubtotalPoints = ci.UnitPoints * ci.Quantity
                });

                _db.Products.Update(p);
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Clear session
            HttpContext.Session.Remove(SESSION_KEY);

            return RedirectToAction("ThankYou", new { id = order.Id });
        }
        catch
        {
            await tx.RollbackAsync();
            TempData["Error"] = "Checkout failed. Please try again.";
            return RedirectToAction("Review");
        }
    }

    // GET: /Shop/ThankYou/{id} - Confirmation page
    public async Task<IActionResult> ThankYou(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Agency)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    // Session helpers
    private OrderDraft? GetDraft()
    {
        var str = HttpContext.Session.GetString(SESSION_KEY);
        if (string.IsNullOrEmpty(str)) return null;
        return JsonSerializer.Deserialize<OrderDraft>(str);
    }

    private void SetDraft(OrderDraft draft)
    {
        HttpContext.Session.SetString(SESSION_KEY, JsonSerializer.Serialize(draft));
    }
}
