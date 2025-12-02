using BlazorBday.Models;

namespace BlazorBday.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task AddAsync(Order order);
    Task UpdateStatusAsync(int id, OrderStatus status);
    Task RefundAsync(int id);
}