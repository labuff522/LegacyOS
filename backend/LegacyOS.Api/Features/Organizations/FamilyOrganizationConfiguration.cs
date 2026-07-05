using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Organizations;

public class FamilyOrganizationConfiguration : IEntityTypeConfiguration<FamilyOrganization>
{
    public void Configure(EntityTypeBuilder<FamilyOrganization> builder)
    {
        builder.ToTable("family_organizations");

        builder.HasKey(x => new { x.FamilyId, x.OrganizationId });

        builder.Property(x => x.FamilyId)
            .HasColumnName("family_id");

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(x => x.JoinedOn)
            .HasColumnName("joined_on");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.HasOne(x => x.Family)
            .WithMany(x => x.FamilyOrganizations)
            .HasForeignKey(x => x.FamilyId);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.FamilyOrganizations)
            .HasForeignKey(x => x.OrganizationId);
    }
}