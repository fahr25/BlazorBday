using System.ComponentModel.DataAnnotations;

namespace BlazorBday.Models;

public class Agency
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Program Admin First Name")]
    public string ProgramAdminFirstName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Program Admin Last Name")]
    public string ProgramAdminLastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Has Received Unique ID")]
    public bool HasReceivedUniqueId { get; set; }

    [Display(Name = "Has Submitted Submissions")]
    public bool HasSubmittedSubmissions { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    [Display(Name = "3-Letter Code")]
    public string ThreeLetterCode { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Population Served")]
    public string PopulationServed { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }

    [StringLength(200)]
    [Display(Name = "Current Executive Director")]
    public string CurrentExecutiveDirector { get; set; } = string.Empty;

    // Navigation property for orders from this agency
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
