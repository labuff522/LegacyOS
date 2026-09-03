using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Sessions;

public class SessionCreditLotConfiguration : IEntityTypeConfiguration<SessionCreditLot>
{
    public void Configure(EntityTypeBuilder<SessionCreditLot> b)
    {
        b.ToTable("session_credit_lots"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("session_credit_lot_id");
        b.Property(x => x.AthleteId).HasColumnName("athlete_id");
        b.Property(x => x.ProductId).HasColumnName("product_id");
        b.Property(x => x.PurchaseOrderId).HasColumnName("purchase_order_id");
        b.Property(x => x.IsUnlimited).HasColumnName("is_unlimited");
        b.Property(x => x.SessionsGranted).HasColumnName("sessions_granted");
        b.Property(x => x.SessionsRemaining).HasColumnName("sessions_remaining");
        b.Property(x => x.GrantedOn).HasColumnName("granted_on");
        b.Property(x => x.ExpiresOn).HasColumnName("expires_on");
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.HasIndex(x => x.PurchaseOrderId).IsUnique();
        b.HasOne(x => x.Athlete).WithMany().HasForeignKey(x => x.AthleteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SessionLedgerEntryConfiguration : IEntityTypeConfiguration<SessionLedgerEntry>
{
    public void Configure(EntityTypeBuilder<SessionLedgerEntry> b)
    {
        b.ToTable("session_ledger_entries"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("session_ledger_entry_id");
        b.Property(x => x.SessionCreditLotId).HasColumnName("session_credit_lot_id");
        b.Property(x => x.AthleteId).HasColumnName("athlete_id");
        b.Property(x => x.StaffPortalUserId).HasColumnName("staff_portal_user_id");
        b.Property(x => x.EntryType).HasColumnName("entry_type").HasConversion<string>().HasMaxLength(25);
        b.Property(x => x.SessionChange).HasColumnName("session_change");
        b.Property(x => x.Note).HasColumnName("note").HasMaxLength(500);
        b.Property(x => x.CreatedOn).HasColumnName("created_on");
        b.HasOne(x => x.SessionCreditLot).WithMany().HasForeignKey(x => x.SessionCreditLotId).OnDelete(DeleteBehavior.Restrict);
    }
}
