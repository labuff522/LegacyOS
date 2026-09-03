using System.Security.Claims;
using System.Text.Encodings.Web;
using LegacyOS.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LegacyOS.Api.Features.Portal;

public class PortalTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    LegacyOSDbContext db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "PortalToken";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var rawToken = header[7..].Trim();
        if (rawToken.Length == 0) return AuthenticateResult.Fail("Missing bearer token.");

        var now = DateTime.UtcNow;
        var tokenHash = TokenUtilities.Hash(rawToken);
        var token = await db.PortalAccessTokens
            .AsNoTracking()
            .Include(x => x.PortalUser)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.RevokedOn == null && x.ExpiresOn > now);

        if (token?.PortalUser is not { IsActive: true } user)
            return AuthenticateResult.Fail("Invalid or expired bearer token.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        if (user.GuardianId is Guid guardianId)
            claims.Add(new("guardian_id", guardianId.ToString()));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationScheme));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthenticationScheme));
    }
}
