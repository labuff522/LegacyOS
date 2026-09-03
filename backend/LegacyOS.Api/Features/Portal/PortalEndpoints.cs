using System.Security.Claims;
using LegacyOS.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Portal;

public static class PortalEndpoints
{
    public static RouteGroupBuilder MapPortalEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/portal/auth");
        auth.MapPost("/register", RegisterAsync).AllowAnonymous();
        auth.MapPost("/login", LoginAsync).AllowAnonymous();
        auth.MapPost("/logout", LogoutAsync).RequireAuthorization();

        var portal = app.MapGroup("/portal").RequireAuthorization("CustomerOnly");
        portal.MapGet("/me", GetCurrentFamilyAsync);

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
            string.IsNullOrWhiteSpace(request.Email) ||
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
                        }).FirstOrDefault()
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
public record CreateInvitationRequest(Guid GuardianId);
public record AuthResponse(string AccessToken, DateTime ExpiresOn, string Email, string Role);
