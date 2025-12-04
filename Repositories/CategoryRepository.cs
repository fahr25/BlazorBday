using Microsoft.EntityFrameworkCore;
using BlazorBday.Data;
using BlazorBday.Models;

namespace BlazorBday.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly MarketShopDbContext _context;

    public CategoryRepository(MarketShopDbContext context)
    {
        _context = context;
    }

    // Get all categories ordered by display order
    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    // Get active categories only
    public async Task<List<Category>> GetActiveAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    // Find category by ID
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Get category with subcategories included
    public async Task<Category?> GetByIdWithSubcategoriesAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Subcategories.OrderBy(s => s.DisplayOrder))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Create new category
    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    // Update existing category
    public async Task UpdateAsync(Category category)
    {
        // Load existing entity to avoid navigation property conflicts
        var existingCategory = await _context.Categories.FindAsync(category.Id);
        if (existingCategory == null) return;

        // Update only scalar properties
        _context.Entry(existingCategory).CurrentValues.SetValues(category);
        await _context.SaveChangesAsync();
    }

    // Delete category
    public async Task DeleteAsync(int id)
    {
        var categoryToDelete = await _context.Categories.FindAsync(id);

        if (categoryToDelete != null)
        {
            _context.Categories.Remove(categoryToDelete);
            await _context.SaveChangesAsync();
        }
    }

    // Check if category exists
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }
}
