using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Products;
using LegacyOS.Api.Features.Purchases;

namespace LegacyOS.Api.Features.Sessions;

public class SessionCreditLot
{
    public Guid Id { get; set; }
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public Guid? GrantedByStaffPortalUserId { get; set; }
    public SessionGrantSource GrantSource { get; set; } = SessionGrantSource.Stripe;
    public bool IsUnlimited { get; set; }
    public int? SessionsGranted { get; set; }
    public int? SessionsRemaining { get; set; }
    public DateTime GrantedOn { get; set; }
    public DateTime ExpiresOn { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SessionLedgerEntry
{
    public Guid Id { get; set; }
    public Guid SessionCreditLotId { get; set; }
    public SessionCreditLot SessionCreditLot { get; set; } = null!;
    public Guid AthleteId { get; set; }
    public Guid? StaffPortalUserId { get; set; }
    public SessionLedgerEntryType EntryType { get; set; }
    public int SessionChange { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public enum SessionLedgerEntryType { Grant = 1, CheckIn = 2, Adjustment = 3, Refund = 4 }
public enum SessionGrantSource { Stripe = 1, PaidOutsideStripe = 2, Complimentary = 3 }
