using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyOS.Api.Features.Portal;

public class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
{
    public void Configure(EntityTypeBuilder<PortalUser> builder)
    {
        builder.ToTable("portal_users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("portal_user_id");
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(255).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(25).IsRequired();
        builder.Property(x => x.GuardianId).HasColumnName("guardian_id");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.HasIndex(x => x.GuardianId).IsUnique();
        builder.HasOne(x => x.Guardian).WithOne().HasForeignKey<PortalUser>(x => x.GuardianId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PortalAccessTokenConfiguration : IEntityTypeConfiguration<PortalAccessToken>
{
    public void Configure(EntityTypeBuilder<PortalAccessToken> builder)
    {
        builder.ToTable("portal_access_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("portal_access_token_id");
        builder.Property(x => x.PortalUserId).HasColumnName("portal_user_id");
        builder.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExpiresOn).HasColumnName("expires_on");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");
        builder.Property(x => x.RevokedOn).HasColumnName("revoked_on");
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasOne(x => x.PortalUser).WithMany().HasForeignKey(x => x.PortalUserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class GuardianInvitationConfiguration : IEntityTypeConfiguration<GuardianInvitation>
{
    public void Configure(EntityTypeBuilder<GuardianInvitation> builder)
    {
        builder.ToTable("guardian_invitations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("guardian_invitation_id");
        builder.Property(x => x.GuardianId).HasColumnName("guardian_id");
        builder.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExpiresOn).HasColumnName("expires_on");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");
        builder.Property(x => x.AcceptedOn).HasColumnName("accepted_on");
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasOne(x => x.Guardian).WithMany().HasForeignKey(x => x.GuardianId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PortalPasswordResetTokenConfiguration : IEntityTypeConfiguration<PortalPasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PortalPasswordResetToken> builder)
    {
        builder.ToTable("portal_password_reset_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("portal_password_reset_token_id");
        builder.Property(x => x.PortalUserId).HasColumnName("portal_user_id");
        builder.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExpiresOn).HasColumnName("expires_on");
        builder.Property(x => x.CreatedOn).HasColumnName("created_on");
        builder.Property(x => x.UsedOn).HasColumnName("used_on");
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasOne(x => x.PortalUser).WithMany().HasForeignKey(x => x.PortalUserId).OnDelete(DeleteBehavior.Cascade);
    }
}
