using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Enrollments;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("enrollment_id");
        builder.Property(x => x.AthleteId).HasColumnName("athlete_id");
        builder.Property(x => x.MembershipPlanId).HasColumnName("membership_plan_id");
        builder.Property(x => x.StartDate).HasColumnName("start_date");
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");

        builder.HasOne(x => x.Athlete)
            .WithMany()
            .HasForeignKey(x => x.AthleteId);

        builder.HasOne(x => x.MembershipPlan)
            .WithMany()
            .HasForeignKey(x => x.MembershipPlanId);
    }
}