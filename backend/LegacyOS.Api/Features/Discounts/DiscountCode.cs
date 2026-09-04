using LegacyOS.Api.Features.Products;

namespace LegacyOS.Api.Features.Discounts;

public class DiscountCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public DateTime? StartsOn { get; set; }
    public DateTime? EndsOn { get; set; }
    public int? MaxRedemptions { get; set; }
    public int RedemptionCount { get; set; }
    public bool IsAutomaticSibling { get; set; }
    public int? SiblingStartPosition { get; set; }
    public int? SiblingEndPosition { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
public enum DiscountType { Percentage = 1, FixedAmount = 2 }
