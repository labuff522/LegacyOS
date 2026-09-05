using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Families;

public class AthleteGroupConfiguration : IEntityTypeConfiguration<AthleteGroup>
{
    public void Configure(EntityTypeBuilder<AthleteGroup> builder)
    {
        builder.ToTable("athlete_groups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("athlete_group_id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
