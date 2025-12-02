using BlazorBday.Models;

namespace BlazorBday.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<List<Category>> GetActiveAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByIdWithSubcategoriesAsync(int id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
