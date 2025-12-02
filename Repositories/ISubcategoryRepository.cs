using BlazorBday.Models;

namespace BlazorBday.Repositories;

public interface ISubcategoryRepository
{
    Task<List<Subcategory>> GetAllAsync();
    Task<List<Subcategory>> GetActiveAsync();
    Task<List<Subcategory>> GetByCategoryIdAsync(int categoryId);
    Task<Subcategory?> GetByIdAsync(int id);
    Task AddAsync(Subcategory subcategory);
    Task UpdateAsync(Subcategory subcategory);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
