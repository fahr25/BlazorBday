using BlazorBday.Models;
using BlazorBday.ViewModels;

namespace BlazorBday.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    // Performance-optimized method for dashboard statistics
    Task<InventoryStatisticsViewModel> GetInventoryStatisticsAsync(int lowStockThreshold);
}