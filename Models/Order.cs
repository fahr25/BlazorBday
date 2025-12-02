using System.ComponentModel.DataAnnotations;

namespace BlazorBday.Models;

public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}

public class Order
{
    [Key]
    public int Id { get; set; }

    // Agency relationship
    public int AgencyId { get; set; }
    public Agency? Agency { get; set; }

    // Child demographic info (no PII except age/DOB for points calculation)
    public DateTime ChildDateOfBirth { get; set; }
    public int ChildAge { get; set; }


    // Points are auto-calculated based on child age: 0-11 = 65pts, 12-18 = 100pts
    public int PointsAssigned { get; set; }
    public int PointsUsed { get; set; }

    public int TotalItems { get; set; }

    // Customer contact info
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}