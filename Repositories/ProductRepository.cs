using Microsoft.EntityFrameworkCore;
using BlazorBday.Data;
using BlazorBday.Models;
using BlazorBday.ViewModels;

namespace BlazorBday.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MarketShopDbContext _db;
    public ProductRepository(MarketShopDbContext db) => _db = db;

    public Task<List<Product>> GetAllAsync() => _db.Products.ToListAsync();

    public Task<Product?> GetByIdAsync(int id) =>
        _db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        // Load existing entity to avoid navigation property conflicts
        var existingProduct = await _db.Products.FindAsync(product.Id);
        if (existingProduct == null) return;

        // Update only scalar properties
        _db.Entry(existingProduct).CurrentValues.SetValues(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p != null)
        {
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
        }
    }

    public Task<bool> ExistsAsync(int id) =>
        _db.Products.AnyAsync(p => p.Id == id);

    // Calculate inventory statistics efficiently using database queries
    public async Task<InventoryStatisticsViewModel> GetInventoryStatisticsAsync(int lowStockThreshold)
    {
        var stats = new InventoryStatisticsViewModel
        {
            LowStockThreshold = lowStockThreshold,
            TotalProducts = await _db.Products.CountAsync(),
            LowStockCount = await _db.Products.CountAsync(p => p.Inventory > 0 && p.Inventory < lowStockThreshold),
            OutOfStockCount = await _db.Products.CountAsync(p => p.Inventory == 0)
        };

        // Group by category and calculate statistics for each
        var categoryStats = await _db.Products
            .Include(p => p.Category)
            .GroupBy(p => p.Category.Name)
            .Select(g => new CategoryInventoryStats
            {
                CategoryName = g.Key,
                ProductCount = g.Count(),
                TotalInventory = g.Sum(p => p.Inventory),
                LowStockCount = g.Count(p => p.Inventory > 0 && p.Inventory < lowStockThreshold)
            })
            .ToListAsync();

        stats.CategoryStats = categoryStats.ToDictionary(c => c.CategoryName, c => c);

        return stats;
    }
}