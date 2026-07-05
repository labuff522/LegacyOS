using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Families;

public class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        builder.ToTable("guardians");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("guardian_id");
        builder.Property(x => x.FamilyId).HasColumnName("family_id");

        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(25);

        builder.Property(x => x.IsPrimaryContact).HasColumnName("is_primary_contact");
        builder.Property(x => x.ReceivesBilling).HasColumnName("receives_billing");
        builder.Property(x => x.ReceivesSms).HasColumnName("receives_sms");

        builder.HasOne(x => x.Family)
            .WithMany(x => x.Guardians)
            .HasForeignKey(x => x.FamilyId);
    }
}