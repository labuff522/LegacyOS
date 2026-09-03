using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace LegacyOS.Api.Features.Discounts;
public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> b)
    {
        b.ToTable("discount_codes"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("discount_code_id");
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired(); b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Description).HasColumnName("description").HasMaxLength(500); b.Property(x => x.DiscountType).HasColumnName("discount_type").HasConversion<string>().HasMaxLength(25);
        b.Property(x => x.Value).HasColumnName("value").HasPrecision(10, 2); b.Property(x => x.ProductId).HasColumnName("product_id");
        b.Property(x => x.StartsOn).HasColumnName("starts_on"); b.Property(x => x.EndsOn).HasColumnName("ends_on"); b.Property(x => x.MaxRedemptions).HasColumnName("max_redemptions");
        b.Property(x => x.RedemptionCount).HasColumnName("redemption_count"); b.Property(x => x.IsActive).HasColumnName("is_active"); b.Property(x => x.CreatedOn).HasColumnName("created_on");
        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
