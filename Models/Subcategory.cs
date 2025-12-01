using System.ComponentModel.DataAnnotations;

namespace BlazorBday.Models;

public class Subcategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    // Foreign key
    public int CategoryId { get; set; }

    // Navigation property
    public Category Category { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
