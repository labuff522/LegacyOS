using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Waivers;

public class WaiverTemplateConfiguration : IEntityTypeConfiguration<WaiverTemplate>
{
    public void Configure(EntityTypeBuilder<WaiverTemplate> b)
    {
        b.ToTable("waiver_templates"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("waiver_template_id");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(x => x.Version).HasColumnName("version");
        b.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        b.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired();
        b.Property(x => x.FileContent).HasColumnName("file_content").IsRequired();
        b.Property(x => x.Sha256).HasColumnName("sha256").HasMaxLength(64).IsRequired();
        b.Property(x => x.IsRequired).HasColumnName("is_required"); b.Property(x => x.IsActive).HasColumnName("is_active");
        b.Property(x => x.CreatedOn).HasColumnName("created_on");
        b.HasIndex(x => new { x.OrganizationId, x.Name, x.Version }).IsUnique();
    }
}

public class WaiverSignatureConfiguration : IEntityTypeConfiguration<WaiverSignature>
{
    public void Configure(EntityTypeBuilder<WaiverSignature> b)
    {
        b.ToTable("waiver_signatures"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("waiver_signature_id"); b.Property(x => x.WaiverTemplateId).HasColumnName("waiver_template_id");
        b.Property(x => x.FamilyId).HasColumnName("family_id"); b.Property(x => x.AthleteId).HasColumnName("athlete_id");
        b.Property(x => x.GuardianId).HasColumnName("guardian_id"); b.Property(x => x.PortalUserId).HasColumnName("portal_user_id");
        b.Property(x => x.SignedName).HasColumnName("signed_name").HasMaxLength(200).IsRequired();
        b.Property(x => x.WaiverSha256).HasColumnName("waiver_sha256").HasMaxLength(64).IsRequired();
        b.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(100); b.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        b.Property(x => x.SignedOn).HasColumnName("signed_on"); b.Property(x => x.ExpiresOn).HasColumnName("expires_on");
        b.HasIndex(x => new { x.WaiverTemplateId, x.AthleteId });
        b.HasOne(x => x.WaiverTemplate).WithMany().HasForeignKey(x => x.WaiverTemplateId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Athlete).WithMany().HasForeignKey(x => x.AthleteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Guardian).WithMany().HasForeignKey(x => x.GuardianId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PortalUser).WithMany().HasForeignKey(x => x.PortalUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
