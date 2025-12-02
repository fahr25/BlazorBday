using Microsoft.EntityFrameworkCore;
using BlazorBday.Data;
using BlazorBday.Models;

namespace BlazorBday.Repositories;

public class SubcategoryRepository : ISubcategoryRepository
{
    private readonly MarketShopDbContext _context;
    public SubcategoryRepository(MarketShopDbContext context)
    {
        _context = context;
    }

    // Get all subcategories with parent category
    public async Task<List<Subcategory>> GetAllAsync()
    {
        return await _context.Subcategories
            .Include(s => s.Category)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    // Get active subcategories only
    public async Task<List<Subcategory>> GetActiveAsync()
    {
        return await _context.Subcategories
            .Include(s => s.Category)
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    // Get subcategories for specific category
    public async Task<List<Subcategory>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Subcategories
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    // Find subcategory by ID with parent category
    public async Task<Subcategory?> GetByIdAsync(int id)
    {
        return await _context.Subcategories
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // Create new subcategory
    public async Task AddAsync(Subcategory subcategory)
    {
        _context.Subcategories.Add(subcategory);
        await _context.SaveChangesAsync();
    }

    // Update existing subcategory
    public async Task UpdateAsync(Subcategory subcategory)
    {
        _context.Subcategories.Update(subcategory);
        await _context.SaveChangesAsync();
    }

    // Delete subcategory
    public async Task DeleteAsync(int id)
    {
        var subcategoryToDelete = await _context.Subcategories.FindAsync(id);

        if (subcategoryToDelete != null)
        {
            _context.Subcategories.Remove(subcategoryToDelete);
            await _context.SaveChangesAsync();
        }
    }

    // Check if subcategory exists
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Subcategories.AnyAsync(s => s.Id == id);
    }
}
