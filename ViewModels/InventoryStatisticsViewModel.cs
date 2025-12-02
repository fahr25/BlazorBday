namespace BlazorBday.ViewModels;

// Summary statistics for dashboard - avoids loading all products
public class InventoryStatisticsViewModel
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int LowStockThreshold { get; set; } = 5;

    // Category breakdown
    public Dictionary<string, CategoryInventoryStats> CategoryStats { get; set; } = new();
}

public class CategoryInventoryStats
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalInventory { get; set; }
    public int LowStockCount { get; set; }
}
