using BlazorBday.Models;

namespace BlazorBday.ViewModels;

public class AdminDashboardViewModel
{
    // Inventory statistics (replaces loading all products)
    public InventoryStatisticsViewModel InventoryStats { get; set; } = new();

    public List<Order> RecentOrders { get; set; } = new();

    public int TotalOrders => RecentOrders?.Count ?? 0;
    public int PendingOrders => RecentOrders?.Count(o => o.Status == OrderStatus.Pending) ?? 0;
}