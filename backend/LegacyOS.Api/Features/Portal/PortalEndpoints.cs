using System.Security.Claims;
using LegacyOS.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Organizations;
using LegacyOS.Api.Features.Waivers;

namespace LegacyOS.Api.Features.Portal;

public static class PortalEndpoints
{
    public static RouteGroupBuilder MapPortalEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/portal/auth");
        auth.MapPost("/register", RegisterAsync).AllowAnonymous();
        auth.MapGet("/registration-options", async (LegacyOSDbContext db) => Results.Ok(new
        {
            waivers = await db.WaiverTemplates.Where(x => x.IsActive).OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name, x.Version, x.FileName, x.IsRequired }).ToListAsync()
        })).AllowAnonymous();
        auth.MapPost("/self-register", SelfRegisterAsync).AllowAnonymous();
        auth.MapPost("/login", LoginAsync).AllowAnonymous();
        auth.MapPost("/forgot-password", ForgotPasswordAsync).AllowAnonymous().RequireRateLimiting("PasswordRecovery");
        auth.MapPost("/reset-password", ResetPasswordAsync).AllowAnonymous();
        auth.MapPost("/logout", LogoutAsync).RequireAuthorization();

        var portal = app.MapGroup("/portal").RequireAuthorization("CustomerOnly");
        portal.MapGet("/me", GetCurrentFamilyAsync);
        portal.MapPut("/account/email", UpdateOwnEmailAsync);
        portal.MapPost("/athletes", AddOwnAthleteAsync);

        app.MapPost("/staff/guardian-invitations", CreateInvitationAsync)
            .RequireAuthorization("StaffOnly");

        return portal;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        LegacyOSDbContext db,
        IPasswordHasher<PortalUser> passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(request.InvitationToken) ||
            !TokenUtilities.IsValidEmail(request.Email) ||
            request.Password.Length < 12)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["account"] = ["A valid invitation, email, and password of at least 12 characters are required."]
            });

        var now = DateTime.UtcNow;
        var normalizedEmail = TokenUtilities.NormalizeEmail(request.Email);
        var invitationHash = TokenUtilities.Hash(request.InvitationToken);

        await using var transaction = await db.Database.BeginTransactionAsync();
        var invitation = await db.GuardianInvitations
            .Include(x => x.Guardian)
            .SingleOrDefaultAsync(x => x.TokenHash == invitationHash);

        if (invitation is null || invitation.AcceptedOn != null || invitation.ExpiresOn <= now ||
            TokenUtilities.NormalizeEmail(invitation.Guardian.Email) != normalizedEmail)
            return Results.BadRequest(new { message = "The invitation is invalid, expired, or does not match this email." });

        if (await db.PortalUsers.AnyAsync(x => x.NormalizedEmail == normalizedEmail || x.GuardianId == invitation.GuardianId))
            return Results.Conflict(new { message = "An account already exists for this guardian." });

        var user = new PortalUser
        {
            Id = Guid.NewGuid(),
            Email = invitation.Guardian.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            GuardianId = invitation.GuardianId,
            Role = PortalRoles.Customer,
            CreatedOn = now
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        invitation.AcceptedOn = now;
        db.PortalUsers.Add(user);

        var response = AddAccessToken(db, user, now);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Results.Ok(response);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        LegacyOSDbContext db,
        IPasswordHasher<PortalUser> passwordHasher)
    {
        var normalizedEmail = TokenUtilities.NormalizeEmail(request.Email ?? string.Empty);
        var user = await db.PortalUsers.SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);
        if (user is null || !user.IsActive ||
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password ?? string.Empty) == PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        var response = AddAccessToken(db, user, DateTime.UtcNow);
        await db.SaveChangesAsync();
        return Results.Ok(response);
    }

    private static async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, LegacyOSDbContext db,
        PasswordResetEmailService emailService, ILogger<PasswordResetEmailService> logger, CancellationToken ct)
    {
        const string message = "If an active account uses that email, a password reset link has been sent.";
        if (!TokenUtilities.IsValidEmail(request.Email)) return Results.Accepted(value: new { message });
        var user = await db.PortalUsers.SingleOrDefaultAsync(x => x.NormalizedEmail == TokenUtilities.NormalizeEmail(request.Email!) && x.IsActive, ct);
        if (user is null) return Results.Accepted(value: new { message });
        var now = DateTime.UtcNow;
        var oldTokens = await db.PortalPasswordResetTokens.Where(x => x.PortalUserId == user.Id && x.UsedOn == null).ToListAsync(ct);
        foreach (var oldToken in oldTokens) oldToken.UsedOn = now;
        var rawToken = TokenUtilities.CreateToken();
        db.PortalPasswordResetTokens.Add(new PortalPasswordResetToken { Id = Guid.NewGuid(), PortalUserId = user.Id,
            TokenHash = TokenUtilities.Hash(rawToken), CreatedOn = now, ExpiresOn = now.AddMinutes(30) });
        await db.SaveChangesAsync(ct);
        try { await emailService.SendAsync(user.Email, rawToken, ct); }
        catch (Exception exception)
        {
            logger.LogError(exception, "Password reset email delivery failed.");
        }
        return Results.Accepted(value: new { message });
    }

    private static async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request, LegacyOSDbContext db,
        IPasswordHasher<PortalUser> passwordHasher, CancellationToken ct)
    {
        if (!TokenUtilities.IsValidEmail(request.Email) || string.IsNullOrWhiteSpace(request.Token) || request.Password is null || request.Password.Length < 12)
            return Results.BadRequest(new { message = "The reset link or new password is invalid." });
        var now = DateTime.UtcNow;
        var tokenHash = TokenUtilities.Hash(request.Token);
        var normalizedEmail = TokenUtilities.NormalizeEmail(request.Email!);
        var reset = await db.PortalPasswordResetTokens.Include(x => x.PortalUser)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.PortalUser.NormalizedEmail == normalizedEmail, ct);
        if (reset is null || reset.UsedOn != null || reset.ExpiresOn <= now || !reset.PortalUser.IsActive)
            return Results.BadRequest(new { message = "The reset link is invalid or expired." });
        reset.UsedOn = now;
        reset.PortalUser.PasswordHash = passwordHasher.HashPassword(reset.PortalUser, request.Password);
        var sessions = await db.PortalAccessTokens.Where(x => x.PortalUserId == reset.PortalUserId && x.RevokedOn == null).ToListAsync(ct);
        foreach (var session in sessions) session.RevokedOn = now;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SelfRegisterAsync(SelfRegisterRequest request, HttpRequest http, LegacyOSDbContext db,
        IPasswordHasher<PortalUser> passwordHasher)
    {
        if (!TokenUtilities.IsValidEmail(request.Email) || request.Password.Length < 12 ||
            string.IsNullOrWhiteSpace(request.FamilyName) || string.IsNullOrWhiteSpace(request.GuardianFirstName) ||
            string.IsNullOrWhiteSpace(request.GuardianLastName) || request.Athletes.Count == 0)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["account"] = ["Complete the family, guardian, athlete, email, and password fields."] });
        var organization = await db.Organizations.Where(x => x.IsActive).OrderBy(x => x.CreatedOn).FirstOrDefaultAsync();
        if (organization is null) return Results.Problem("This installation has no internal organization record.");
        var requiredWaivers = await db.WaiverTemplates.Where(x => x.IsActive && x.IsRequired).ToListAsync();
        if (requiredWaivers.Count > 0 &&
            (string.IsNullOrWhiteSpace(request.SignedName) || requiredWaivers.Any(x => !request.AcceptedWaiverIds.Contains(x.Id))))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["waivers"] = ["Every required waiver must be reviewed and accepted."] });
        var normalizedEmail = TokenUtilities.NormalizeEmail(request.Email);
        if (await db.PortalUsers.AnyAsync(x => x.NormalizedEmail == normalizedEmail))
            return Results.Conflict(new { message = "An account already exists for this email." });

        await using var transaction = await db.Database.BeginTransactionAsync();
        var family = new Family { Id = Guid.NewGuid(), FamilyName = request.FamilyName.Trim(), IsActive = true, CreatedOn = DateTime.UtcNow };
        var guardian = new Guardian { Id = Guid.NewGuid(), Family = family, FamilyId = family.Id,
            FirstName = request.GuardianFirstName.Trim(), LastName = request.GuardianLastName.Trim(), Email = request.Email.Trim(),
            Phone = request.Phone.Trim(), IsPrimaryContact = true, ReceivesBilling = true, ReceivesSms = true };
        var athletes = request.Athletes.Select(x => new Athlete { Id = Guid.NewGuid(), Family = family, FamilyId = family.Id,
            FirstName = x.FirstName.Trim(), LastName = x.LastName.Trim(), DateOfBirth = x.DateOfBirth, Gender = x.Gender?.Trim() }).ToList();
        var user = new PortalUser { Id = Guid.NewGuid(), Email = guardian.Email, NormalizedEmail = normalizedEmail,
            Guardian = guardian, GuardianId = guardian.Id, Role = PortalRoles.Customer, CreatedOn = DateTime.UtcNow };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Families.Add(family); db.Guardians.Add(guardian); db.Athletes.AddRange(athletes);
        db.FamilyOrganizations.Add(new FamilyOrganization { Family = family, FamilyId = family.Id,
            Organization = organization, OrganizationId = organization.Id, JoinedOn = DateTime.UtcNow, IsActive = true });
        db.PortalUsers.Add(user);
        var signedOn = DateTime.UtcNow;
        foreach (var athlete in athletes)
        foreach (var waiver in requiredWaivers)
            db.WaiverSignatures.Add(new WaiverSignature { Id = Guid.NewGuid(), WaiverTemplate = waiver,
                WaiverTemplateId = waiver.Id, Family = family, FamilyId = family.Id, Athlete = athlete, AthleteId = athlete.Id,
                Guardian = guardian, GuardianId = guardian.Id, PortalUser = user, PortalUserId = user.Id,
                SignedName = request.SignedName.Trim(), WaiverSha256 = waiver.Sha256,
                IpAddress = http.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                UserAgent = http.Headers.UserAgent.ToString(), SignedOn = signedOn, ExpiresOn = signedOn.AddDays(365) });
        var response = AddAccessToken(db, user, signedOn);
        await db.SaveChangesAsync(); await transaction.CommitAsync(); return Results.Ok(response);
    }

    private static async Task<IResult> LogoutAsync(HttpRequest request, LegacyOSDbContext db)
    {
        var rawToken = request.Headers.Authorization.ToString()[7..].Trim();
        var tokenHash = TokenUtilities.Hash(rawToken);
        var token = await db.PortalAccessTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash);
        if (token is not null)
        {
            token.RevokedOn = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateOwnEmailAsync(UpdateOwnEmailRequest request, ClaimsPrincipal principal,
        LegacyOSDbContext db, IPasswordHasher<PortalUser> passwordHasher)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Forbid();
        if (!TokenUtilities.IsValidEmail(request.NewEmail))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["email"] = ["Enter a valid email address."] });
        var user = await db.PortalUsers.Include(x => x.Guardian).SingleOrDefaultAsync(x => x.Id == userId);
        if (user?.Guardian is null) return Results.NotFound();
        if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword ?? string.Empty) == PasswordVerificationResult.Failed)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["password"] = ["The current password is incorrect."] });
        var normalizedEmail = TokenUtilities.NormalizeEmail(request.NewEmail);
        if (await db.PortalUsers.AnyAsync(x => x.Id != user.Id && x.NormalizedEmail == normalizedEmail))
            return Results.Conflict(new { message = "An account already exists for this email." });
        user.Email = request.NewEmail.Trim(); user.NormalizedEmail = normalizedEmail; user.Guardian.Email = user.Email;
        await db.SaveChangesAsync();
        return Results.Ok(new { user.Email });
    }

    private static async Task<IResult> AddOwnAthleteAsync(AddOwnAthleteRequest request, ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        if (!Guid.TryParse(principal.FindFirstValue("guardian_id"), out var guardianId)) return Results.Forbid();
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
            request.DateOfBirth == default || request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow) ||
            string.IsNullOrWhiteSpace(request.UsaWrestlingMembershipNumber))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["athlete"] = ["Name, date of birth, and USA Wrestling membership number are required."] });
        var familyId = await db.Guardians.Where(x => x.Id == guardianId && x.Family.IsActive).Select(x => (Guid?)x.FamilyId).SingleOrDefaultAsync();
        if (familyId is null) return Results.Forbid();
        var athlete = new Athlete { Id = Guid.NewGuid(), FamilyId = familyId.Value, FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(), DateOfBirth = request.DateOfBirth, Gender = request.Gender?.Trim() };
        db.Athletes.Add(athlete);
        db.UsaWrestlingVerifications.Add(new Features.UsaWrestling.UsaWrestlingVerification { Id = Guid.NewGuid(), Athlete = athlete,
            AthleteId = athlete.Id, MembershipNumber = request.UsaWrestlingMembershipNumber.Trim(),
            Status = Features.UsaWrestling.UsaWrestlingVerificationStatus.Pending, SubmittedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return Results.Created($"/portal/athletes/{athlete.Id}", new { athlete.Id });
    }

    private static async Task<IResult> GetCurrentFamilyAsync(ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        if (!Guid.TryParse(principal.FindFirstValue("guardian_id"), out var guardianId))
            return Results.Forbid();

        var family = await db.Guardians
            .Where(g => g.Id == guardianId && g.Family.IsActive)
            .Select(g => new
            {
                Guardian = new { g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.IsPrimaryContact },
                Family = new
                {
                    g.Family.Id,
                    g.Family.FamilyName,
                    g.Family.IsActive,
                    Guardians = g.Family.Guardians.Select(x => new
                    {
                        x.Id, x.FirstName, x.LastName, x.Email, x.Phone, x.IsPrimaryContact
                    }),
                    Athletes = g.Family.Athletes.Select(x => new
                    {
                        x.Id, x.FirstName, x.LastName, x.DateOfBirth, x.Gender,
                        UsaWrestling = db.UsaWrestlingVerifications.Where(v => v.AthleteId == x.Id).Select(v => new
                        {
                            v.MembershipNumber, Status = v.Status.ToString(), v.SubmittedOn, v.VerifiedOn, v.ExpiresOn
                        }).FirstOrDefault(),
                        SessionPackages = db.SessionCreditLots.Where(l => l.AthleteId == x.Id && l.IsActive)
                            .OrderBy(l => l.ExpiresOn).Select(l => new { l.Id, productName = l.Product.Name,
                                l.IsUnlimited, l.SessionsRemaining, l.ExpiresOn }).ToList()
                    })
                }
            })
            .SingleOrDefaultAsync();

        return family is null ? Results.NotFound() : Results.Ok(family);
    }

    private static async Task<IResult> CreateInvitationAsync(CreateInvitationRequest request, LegacyOSDbContext db)
    {
        var guardian = await db.Guardians.SingleOrDefaultAsync(x => x.Id == request.GuardianId);
        if (guardian is null) return Results.NotFound();
        if (await db.PortalUsers.AnyAsync(x => x.GuardianId == guardian.Id))
            return Results.Conflict(new { message = "This guardian already has an account." });

        var rawToken = TokenUtilities.CreateToken();
        var now = DateTime.UtcNow;
        db.GuardianInvitations.Add(new GuardianInvitation
        {
            Id = Guid.NewGuid(), GuardianId = guardian.Id, TokenHash = TokenUtilities.Hash(rawToken),
            CreatedOn = now, ExpiresOn = now.AddHours(48)
        });
        await db.SaveChangesAsync();
        return Results.Ok(new { invitationToken = rawToken, guardian.Email, expiresOn = now.AddHours(48) });
    }

    private static AuthResponse AddAccessToken(LegacyOSDbContext db, PortalUser user, DateTime now)
    {
        var rawToken = TokenUtilities.CreateToken();
        var expiresOn = now.AddHours(12);
        db.PortalAccessTokens.Add(new PortalAccessToken
        {
            Id = Guid.NewGuid(), PortalUser = user, PortalUserId = user.Id,
            TokenHash = TokenUtilities.Hash(rawToken), CreatedOn = now, ExpiresOn = expiresOn
        });
        return new AuthResponse(rawToken, expiresOn, user.Email, user.Role);
    }
}

public record RegisterRequest(string InvitationToken, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record ForgotPasswordRequest(string? Email);
public record ResetPasswordRequest(string? Email, string? Token, string? Password);
public record UpdateOwnEmailRequest(string NewEmail, string CurrentPassword);
public record AddOwnAthleteRequest(string FirstName, string LastName, DateOnly DateOfBirth, string? Gender, string UsaWrestlingMembershipNumber);
public record CreateInvitationRequest(Guid GuardianId);
public record AuthResponse(string AccessToken, DateTime ExpiresOn, string Email, string Role);
public record SelfRegisterRequest(string FamilyName, string GuardianFirstName, string GuardianLastName, string Email,
    string Phone, string Password, string SignedName, List<Guid> AcceptedWaiverIds, List<SelfRegisterAthlete> Athletes);
public record SelfRegisterAthlete(string FirstName, string LastName, DateOnly DateOfBirth, string? Gender);
