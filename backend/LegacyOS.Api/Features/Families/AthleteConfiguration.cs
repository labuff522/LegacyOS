using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Families;

public class AthleteConfiguration : IEntityTypeConfiguration<Athlete>
{
    public void Configure(EntityTypeBuilder<Athlete> builder)
    {
        builder.ToTable("athletes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("athlete_id");
        builder.Property(x => x.FamilyId).HasColumnName("family_id");

        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(25);

        builder.HasOne(x => x.Family)
            .WithMany(x => x.Athletes)
            .HasForeignKey(x => x.FamilyId);
    }
}