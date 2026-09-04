using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LegacyOS.Api.Features.Portal;

namespace LegacyOS.Api.Features.Purchases;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.ToTable("purchase_orders"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("purchase_order_id");
        b.Property(x => x.PortalUserId).HasColumnName("portal_user_id");
        b.Property(x => x.FamilyId).HasColumnName("family_id");
        b.Property(x => x.AthleteId).HasColumnName("athlete_id");
        b.Property(x => x.MembershipPlanId).HasColumnName("membership_plan_id");
        b.Property(x => x.ProductId).HasColumnName("product_id");
        b.Property(x => x.EnrollmentId).HasColumnName("enrollment_id");
        b.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(25);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(25);
        b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(10, 2);
        b.Property(x => x.OriginalAmount).HasColumnName("original_amount").HasPrecision(10, 2);
        b.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasPrecision(10, 2);
        b.Property(x => x.DiscountCodeId).HasColumnName("discount_code_id");
        b.Property(x => x.DiscountCodeSnapshot).HasColumnName("discount_code_snapshot").HasMaxLength(50);
        b.Property(x => x.FamilySnapshotJson).HasColumnName("family_snapshot_json").HasColumnType("jsonb");
        b.Property(x => x.AthleteSnapshotJson).HasColumnName("athlete_snapshot_json").HasColumnType("jsonb");
        b.Property(x => x.ItemSnapshotJson).HasColumnName("item_snapshot_json").HasColumnType("jsonb");
        b.Property(x => x.DiscountRedemptionRecorded).HasColumnName("discount_redemption_recorded");
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        b.Property(x => x.StripeCheckoutSessionId).HasColumnName("stripe_checkout_session_id").HasMaxLength(255);
        b.Property(x => x.StripeCustomerId).HasColumnName("stripe_customer_id").HasMaxLength(255);
        b.Property(x => x.StripeSubscriptionId).HasColumnName("stripe_subscription_id").HasMaxLength(255);
        b.Property(x => x.CreatedOn).HasColumnName("created_on");
        b.Property(x => x.CompletedOn).HasColumnName("completed_on");
        b.Property(x => x.InstallmentCount).HasColumnName("installment_count");
        b.Property(x => x.InstallmentAmount).HasColumnName("installment_amount").HasPrecision(10, 2);
        b.Property(x => x.IsPaymentCurrent).HasColumnName("is_payment_current");
        b.Property(x => x.BillingDayOfMonth).HasColumnName("billing_day_of_month");
        b.HasIndex(x => x.StripeCheckoutSessionId).IsUnique();
        b.HasOne<PortalUser>().WithMany().HasForeignKey(x => x.PortalUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Athlete).WithMany().HasForeignKey(x => x.AthleteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MembershipPlan).WithMany().HasForeignKey(x => x.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Enrollment).WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DiscountCode).WithMany().HasForeignKey(x => x.DiscountCodeId).OnDelete(DeleteBehavior.Restrict);
    }
}
