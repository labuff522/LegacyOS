using LegacyOS.Api.Features.Enrollments;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Memberships;
using LegacyOS.Api.Features.Products;
using LegacyOS.Api.Features.Discounts;

namespace LegacyOS.Api.Features.Purchases;

public class PurchaseOrder
{
    public Guid Id { get; set; }
    public Guid PortalUserId { get; set; }
    public Guid FamilyId { get; set; }
    public Family Family { get; set; } = null!;
    public Guid? AthleteId { get; set; }
    public Athlete? Athlete { get; set; }
    public Guid? MembershipPlanId { get; set; }
    public MembershipPlan? MembershipPlan { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid? EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public PurchaseKind Kind { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public decimal Amount { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public Guid? DiscountCodeId { get; set; }
    public DiscountCode? DiscountCode { get; set; }
    public string? DiscountCodeSnapshot { get; set; }
    public string FamilySnapshotJson { get; set; } = "{}";
    public string? AthleteSnapshotJson { get; set; }
    public string ItemSnapshotJson { get; set; } = "{}";
    public string Currency { get; set; } = "usd";
    public string? StripeCheckoutSessionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedOn { get; set; }
    public bool DiscountRedemptionRecorded { get; set; }
}

public enum PurchaseKind { MembershipPlan = 1, Product = 2 }
public enum PurchaseStatus { Pending = 1, Completed = 2, Failed = 3, Expired = 4 }
