using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BlazorBday.Data;
using BlazorBday.Models;
using BlazorBday.Repositories;
using BlazorBday.ViewModels;

namespace BlazorBday.Controllers;


[Authorize] // Require authentication for all admin actions
public class AdminController : Controller
{
    private readonly IProductRepository _repo;
    private readonly IOrderRepository _orders;
    private readonly ICategoryRepository _categories;
    private readonly ISubcategoryRepository _subcategories;
    private readonly IAgencyRepository _agencies;

    public AdminController(
        IProductRepository repo,
        IOrderRepository orders,
        ICategoryRepository categories,
        ISubcategoryRepository subcategories,
        IAgencyRepository agencies)
    {
        _repo = repo;
        _orders = orders;
        _categories = categories;
        _subcategories = subcategories;
        _agencies = agencies;
    }

    // GET: /Admin - Dashboard with summary statistics and recent orders
    public async Task<IActionResult> Index()
    {
        const int LOW_STOCK_THRESHOLD = 5;

        // Load only summary statistics (performance optimized)
        var inventoryStats = await _repo.GetInventoryStatisticsAsync(LOW_STOCK_THRESHOLD);
        var orders = (await _orders.GetAllAsync()).Take(20).ToList();

        var adminDashboardViewModel = new AdminDashboardViewModel
        {
            InventoryStats = inventoryStats,
            RecentOrders = orders
        };

        return View(adminDashboardViewModel);
    }

    // GET: /Admin/Inventory - Full inventory management view
    public async Task<IActionResult> Inventory()
    {
        var products = await _repo.GetAllAsync();
        return View("Products/Index", products);
    }

    // ========== PRODUCT MANAGEMENT ==========

    // GET: /Admin/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var product = await _repo.GetByIdAsync(id.Value);
        if (product == null) return NotFound();
        return View("Products/Details", product);
    }

    // GET: /Admin/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _categories.GetActiveAsync();
        ViewBag.Subcategories = await _subcategories.GetActiveAsync();
        return View("Products/Create", new Product());
    }

    // POST: /Admin/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categories.GetActiveAsync();
            ViewBag.Subcategories = await _subcategories.GetActiveAsync();
            return View("Products/Create", product);
        }
        await _repo.AddAsync(product);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var product = await _repo.GetByIdAsync(id.Value);
        if (product == null) return NotFound();

        ViewBag.Categories = await _categories.GetActiveAsync();
        ViewBag.Subcategories = await _subcategories.GetActiveAsync();
        return View("Products/Edit", product);
    }

    // POST: /Admin/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categories.GetActiveAsync();
            ViewBag.Subcategories = await _subcategories.GetActiveAsync();
            return View("Products/Edit", product);
        }

        try
        {
            await _repo.UpdateAsync(product);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _repo.ExistsAsync(id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var product = await _repo.GetByIdAsync(id.Value);
        if (product == null) return NotFound();
        return View("Products/Delete", product);
    }

    // POST: /Admin/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // ========== CATEGORY MANAGEMENT ==========

    // GET: /Admin/CategoriesHelp
    public IActionResult CategoriesHelp()
    {
        return View("Categories/Help");
    }

    // GET: /Admin/Categories
    public async Task<IActionResult> Categories()
    {
        var categories = await _categories.GetAllAsync();
        return View("Categories/Index", categories);
    }

    // GET: /Admin/CategoryDetails/5
    public async Task<IActionResult> CategoryDetails(int? id)
    {
        if (id == null) return NotFound();
        var category = await _categories.GetByIdWithSubcategoriesAsync(id.Value);
        if (category == null) return NotFound();
        return View("Categories/Details", category);
    }

    // GET: /Admin/CreateCategory
    public IActionResult CreateCategory()
    {
        return View("Categories/Create", new Category());
    }

    // POST: /Admin/CreateCategory
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (!ModelState.IsValid) return View("Categories/Create", category);
        await _categories.AddAsync(category);
        return RedirectToAction(nameof(Categories));
    }

    // GET: /Admin/EditCategory/5
    public async Task<IActionResult> EditCategory(int? id)
    {
        if (id == null) return NotFound();
        var category = await _categories.GetByIdAsync(id.Value);
        if (category == null) return NotFound();
        return View("Categories/Edit", category);
    }

    // POST: /Admin/EditCategory/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, Category category)
    {
        if (id != category.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Categories/Edit", category);

        try
        {
            await _categories.UpdateAsync(category);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _categories.ExistsAsync(id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Categories));
    }

    // GET: /Admin/DeleteCategory/5
    public async Task<IActionResult> DeleteCategory(int? id)
    {
        if (id == null) return NotFound();
        var category = await _categories.GetByIdAsync(id.Value);
        if (category == null) return NotFound();
        return View("Categories/Delete", category);
    }

    // POST: /Admin/DeleteCategory/5
    [HttpPost, ActionName("DeleteCategory"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirmed(int id)
    {
        await _categories.DeleteAsync(id);
        return RedirectToAction(nameof(Categories));
    }

    // ========== SUBCATEGORY MANAGEMENT ==========

    // GET: /Admin/Subcategories
    public async Task<IActionResult> Subcategories()
    {
        var subcategories = await _subcategories.GetAllAsync();
        return View("Subcategories/Index", subcategories);
    }

    // GET: /Admin/SubcategoryDetails/5
    public async Task<IActionResult> SubcategoryDetails(int? id)
    {
        if (id == null) return NotFound();
        var subcategory = await _subcategories.GetByIdAsync(id.Value);
        if (subcategory == null) return NotFound();
        return View("Subcategories/Details", subcategory);
    }

    // GET: /Admin/CreateSubcategory
    public async Task<IActionResult> CreateSubcategory()
    {
        ViewBag.Categories = await _categories.GetActiveAsync();
        return View("Subcategories/Create", new Subcategory());
    }

    // POST: /Admin/CreateSubcategory
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubcategory(Subcategory subcategory)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categories.GetActiveAsync();
            return View("Subcategories/Create", subcategory);
        }

        await _subcategories.AddAsync(subcategory);
        return RedirectToAction(nameof(Subcategories));
    }

    // GET: /Admin/EditSubcategory/5
    public async Task<IActionResult> EditSubcategory(int? id)
    {
        if (id == null) return NotFound();
        var subcategory = await _subcategories.GetByIdAsync(id.Value);
        if (subcategory == null) return NotFound();

        ViewBag.Categories = await _categories.GetActiveAsync();
        return View("Subcategories/Edit", subcategory);
    }

    // POST: /Admin/EditSubcategory/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubcategory(int id, Subcategory subcategory)
    {
        if (id != subcategory.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categories.GetActiveAsync();
            return View("Subcategories/Edit", subcategory);
        }

        try
        {
            await _subcategories.UpdateAsync(subcategory);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _subcategories.ExistsAsync(id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Subcategories));
    }

    // GET: /Admin/DeleteSubcategory/5
    public async Task<IActionResult> DeleteSubcategory(int? id)
    {
        if (id == null) return NotFound();
        var subcategory = await _subcategories.GetByIdAsync(id.Value);
        if (subcategory == null) return NotFound();
        return View("Subcategories/Delete", subcategory);
    }

    // POST: /Admin/DeleteSubcategory/5
    [HttpPost, ActionName("DeleteSubcategory"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubcategoryConfirmed(int id)
    {
        await _subcategories.DeleteAsync(id);
        return RedirectToAction(nameof(Subcategories));
    }

    // ========== AGENCY MANAGEMENT ==========

    // GET: /Admin/Agencies
    public async Task<IActionResult> Agencies()
    {
        var agencies = await _agencies.GetAllAsync();
        return View("Agencies/Index", agencies);
    }

    // GET: /Admin/AgencyDetails/5
    public async Task<IActionResult> AgencyDetails(int? id)
    {
        if (id == null) return NotFound();
        var agency = await _agencies.GetByIdAsync(id.Value);
        if (agency == null) return NotFound();
        return View("Agencies/Details", agency);
    }

    // GET: /Admin/CreateAgency
    public IActionResult CreateAgency()
    {
        return View("Agencies/Create", new Agency());
    }

    // POST: /Admin/CreateAgency
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAgency(Agency agency)
    {
        if (!ModelState.IsValid) return View("Agencies/Create", agency);
        await _agencies.AddAsync(agency);
        return RedirectToAction(nameof(Agencies));
    }

    // GET: /Admin/EditAgency/5
    public async Task<IActionResult> EditAgency(int? id)
    {
        if (id == null) return NotFound();
        var agency = await _agencies.GetByIdAsync(id.Value);
        if (agency == null) return NotFound();
        return View("Agencies/Edit", agency);
    }

    // POST: /Admin/EditAgency/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAgency(int id, Agency agency)
    {
        if (id != agency.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Agencies/Edit", agency);

        try
        {
            await _agencies.UpdateAsync(agency);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _agencies.ExistsAsync(id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Agencies));
    }

    // GET: /Admin/DeleteAgency/5
    public async Task<IActionResult> DeleteAgency(int? id)
    {
        if (id == null) return NotFound();
        var agency = await _agencies.GetByIdAsync(id.Value);
        if (agency == null) return NotFound();
        return View("Agencies/Delete", agency);
    }

    // POST: /Admin/DeleteAgency/5
    [HttpPost, ActionName("DeleteAgency"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAgencyConfirmed(int id)
    {
        await _agencies.DeleteAsync(id);
        return RedirectToAction(nameof(Agencies));
    }

    // ========== ORDER MANAGEMENT ==========

    // GET: /Admin/Orders - Show all orders
    public async Task<IActionResult> Orders()
    {
        var orders = await _orders.GetAllAsync();
        return View("Orders/Index", orders);
    }

    // GET: /Admin/OrderDetails/5 - Show single order details
    public async Task<IActionResult> OrderDetails(int? id)
    {
        if (id == null) return NotFound();
        var order = await _orders.GetByIdAsync(id.Value);
        if (order == null) return NotFound();
        return View("Orders/Details", order);
    }

    // POST: /Admin/ChangeStatus - Update order status
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            TempData["Error"] = "Invalid order status";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        try
        {
            await _orders.UpdateStatusAsync(id, orderStatus);
            TempData["Success"] = $"Order status updated to {status}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to update order: {ex.Message}";
        }

        return RedirectToAction(nameof(OrderDetails), new { id });
    }

    // POST: /Admin/Refund - Refund order and restore inventory
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(int id)
    {
        try
        {
            await _orders.RefundAsync(id);
            TempData["Success"] = "Order refunded successfully and inventory restored";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to refund order: {ex.Message}";
        }

        return RedirectToAction(nameof(OrderDetails), new { id });
    }
}