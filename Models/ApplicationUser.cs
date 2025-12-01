using Microsoft.AspNetCore.Identity;

namespace BlazorBday.Models;

// Extends IdentityUser for admin authentication
public class ApplicationUser : IdentityUser
{
    // Add custom properties here if needed in the future
    // Example: public string? FullName { get; set; }
}
