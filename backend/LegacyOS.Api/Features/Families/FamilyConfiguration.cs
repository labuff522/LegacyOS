using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Families;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("family_id");

        builder.Property(x => x.FamilyName)
            .HasColumnName("family_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.PrimaryContactName)
            .HasColumnName("primary_contact_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(25);

        builder.Property(x => x.BillingAddress)
            .HasColumnName("billing_address")
            .HasMaxLength(500);

        builder.Property(x => x.EmergencyContactName)
            .HasColumnName("emergency_contact_name")
            .HasMaxLength(150);

        builder.Property(x => x.EmergencyContactPhone)
            .HasColumnName("emergency_contact_phone")
            .HasMaxLength(25);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedOn)
            .HasColumnName("created_on");

        builder.Property(x => x.ModifiedOn)
            .HasColumnName("modified_on");
    }
}