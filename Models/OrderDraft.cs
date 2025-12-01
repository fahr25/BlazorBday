namespace BlazorBday.Models;

public class OrderDraft
{
    // Step 1: Agency selection
    public int? AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public string ThreeLetterCode { get; set; } = string.Empty;

    // Step 2: Child demographic info (no PII)
    public DateTime? ChildDateOfBirth { get; set; }
    public int ChildAge { get; set; }

    // Auto-calculated points based on child age
    // 0-11 years = 65 points, 12-18 years = 100 points
    public int PointsAssigned => ChildAge >= 0 && ChildAge <= 11 ? 65 :
                                  ChildAge >= 12 && ChildAge <= 18 ? 100 : 0;

    // Shopping cart
    public List<CartItem> Items { get; set; } = new();
    public int PointsUsed => Items.Sum(i => i.Subtotal);
    public int PointsRemaining => PointsAssigned - PointsUsed;

    // Validation helpers for sequential flow
    public bool HasCard => Items.Any(i => i.CategoryName == "Cards");
    public bool HasBook => Items.Any(i => i.CategoryName == "Books");
    public bool HasTreat => Items.Any(i => i.CategoryName == "Treats");
    public bool CanSelectGifts => HasCard && HasBook && HasTreat;

    
}