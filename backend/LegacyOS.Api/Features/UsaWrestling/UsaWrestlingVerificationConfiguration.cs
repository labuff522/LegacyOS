using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.UsaWrestling;

public class UsaWrestlingVerificationConfiguration : IEntityTypeConfiguration<UsaWrestlingVerification>
{
    public void Configure(EntityTypeBuilder<UsaWrestlingVerification> b)
    {
        b.ToTable("usa_wrestling_verifications"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("usa_wrestling_verification_id");
        b.Property(x => x.AthleteId).HasColumnName("athlete_id");
        b.Property(x => x.MembershipNumber).HasColumnName("membership_number").HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.SubmittedOn).HasColumnName("submitted_on");
        b.Property(x => x.VerifiedOn).HasColumnName("verified_on");
        b.Property(x => x.ExpiresOn).HasColumnName("expires_on");
        b.Property(x => x.VerifiedByPortalUserId).HasColumnName("verified_by_portal_user_id");
        b.Property(x => x.StaffNotes).HasColumnName("staff_notes").HasMaxLength(500);
        b.HasIndex(x => x.AthleteId).IsUnique(); b.HasIndex(x => x.MembershipNumber).IsUnique();
        b.HasOne(x => x.Athlete).WithOne().HasForeignKey<UsaWrestlingVerification>(x => x.AthleteId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.VerifiedByPortalUser).WithMany().HasForeignKey(x => x.VerifiedByPortalUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
