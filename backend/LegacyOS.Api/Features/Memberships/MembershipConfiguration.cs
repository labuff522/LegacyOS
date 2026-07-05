using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Memberships;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("service_id");
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId);
    }
}

public class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ToTable("membership_plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("membership_plan_id");
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.MonthlyPrice).HasColumnName("monthly_price").HasPrecision(10, 2);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId);
    }
}

public class PlanServiceConfiguration : IEntityTypeConfiguration<PlanService>
{
    public void Configure(EntityTypeBuilder<PlanService> builder)
    {
        builder.ToTable("plan_services");

        builder.HasKey(x => new { x.MembershipPlanId, x.ServiceId });

        builder.Property(x => x.MembershipPlanId).HasColumnName("membership_plan_id");
        builder.Property(x => x.ServiceId).HasColumnName("service_id");

        builder.HasOne(x => x.MembershipPlan)
            .WithMany(x => x.PlanServices)
            .HasForeignKey(x => x.MembershipPlanId);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.PlanServices)
            .HasForeignKey(x => x.ServiceId);
    }
}