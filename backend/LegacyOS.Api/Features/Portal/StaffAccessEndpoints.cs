using System.Security.Claims;
using LegacyOS.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Portal;

public static class StaffAccessEndpoints
{
    public static IEndpointRouteBuilder MapStaffAccessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/staff/access-users").RequireAuthorization("StaffOnly");
        group.MapGet("/", ListAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}/password", ResetPasswordAsync);
        group.MapPut("/{id:guid}/status", SetStatusAsync);
        return app;
    }

    private static async Task<IResult> ListAsync(LegacyOSDbContext db) => Results.Ok(await db.PortalUsers
        .AsNoTracking().Where(x => x.Role == PortalRoles.Staff).OrderBy(x => x.Email)
        .Select(x => new { x.Id, x.Email, x.IsActive, x.CreatedOn }).ToListAsync());

    private static async Task<IResult> CreateAsync(CreateStaffRequest request, LegacyOSDbContext db,
        IPasswordHasher<PortalUser> passwordHasher)
    {
        if (!TokenUtilities.IsValidEmail(request.Email))
            return Results.BadRequest(new { message = "Enter a valid email address." });
        if (request.Password is null || request.Password.Length < 12)
            return Results.BadRequest(new { message = "Temporary password must be at least 12 characters." });

        var normalizedEmail = TokenUtilities.NormalizeEmail(request.Email!);
        if (await db.PortalUsers.AnyAsync(x => x.NormalizedEmail == normalizedEmail))
            return Results.Conflict(new { message = "An account already uses this email address." });

        var user = new PortalUser
        {
            Id = Guid.NewGuid(), Email = request.Email!.Trim(), NormalizedEmail = normalizedEmail,
            Role = PortalRoles.Staff, IsActive = true, CreatedOn = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.PortalUsers.Add(user);
        await db.SaveChangesAsync();
        return Results.Created($"/staff/access-users/{user.Id}", new { user.Id, user.Email, user.IsActive, user.CreatedOn });
    }

    private static async Task<IResult> ResetPasswordAsync(Guid id, ResetStaffPasswordRequest request,
        LegacyOSDbContext db, IPasswordHasher<PortalUser> passwordHasher)
    {
        if (request.Password is null || request.Password.Length < 12)
            return Results.BadRequest(new { message = "Temporary password must be at least 12 characters." });
        var user = await db.PortalUsers.SingleOrDefaultAsync(x => x.Id == id && x.Role == PortalRoles.Staff);
        if (user is null) return Results.NotFound();

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        await RevokeTokensAsync(id, db);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> SetStatusAsync(Guid id, SetStaffStatusRequest request,
        ClaimsPrincipal principal, LegacyOSDbContext db)
    {
        var user = await db.PortalUsers.SingleOrDefaultAsync(x => x.Id == id && x.Role == PortalRoles.Staff);
        if (user is null) return Results.NotFound();
        if (!request.IsActive)
        {
            if (Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var currentId) && currentId == id)
                return Results.BadRequest(new { message = "You cannot deactivate your own account." });
            if (await db.PortalUsers.CountAsync(x => x.Role == PortalRoles.Staff && x.IsActive) <= 1)
                return Results.BadRequest(new { message = "At least one active administrator is required." });
            await RevokeTokensAsync(id, db);
        }
        user.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task RevokeTokensAsync(Guid userId, LegacyOSDbContext db)
    {
        var now = DateTime.UtcNow;
        var tokens = await db.PortalAccessTokens.Where(x => x.PortalUserId == userId && x.RevokedOn == null).ToListAsync();
        foreach (var token in tokens) token.RevokedOn = now;
    }

    public sealed record CreateStaffRequest(string? Email, string? Password);
    public sealed record ResetStaffPasswordRequest(string? Password);
    public sealed record SetStaffStatusRequest(bool IsActive);
}
