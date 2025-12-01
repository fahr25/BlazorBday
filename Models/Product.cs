using System.ComponentModel.DataAnnotations;

namespace BlazorBday.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Image { get; set; }

    public int Points { get; set; }
    public int Inventory { get; set; }

    public int MinAge { get; set; }
    public int MaxAge { get; set; }

    // Foreign keys
    public int CategoryId { get; set; }
    public int? SubcategoryId { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Subcategory? Subcategory { get; set; }
}
