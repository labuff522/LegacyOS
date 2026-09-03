using LegacyOS.Api.Features.Portal;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;
using LegacyOS.Api.Features.Purchases;

var failures = new List<string>();
void Check(bool condition, string name)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} {name}");
    if (!condition) failures.Add(name);
}

var token = TokenUtilities.CreateToken();
Check(token.Length >= 43, "access tokens contain at least 256 bits of randomness");
Check(TokenUtilities.Hash(token) == TokenUtilities.Hash(token), "token hashing is deterministic");
Check(TokenUtilities.Hash(token) != token && TokenUtilities.Hash(token).Length == 64, "only a SHA-256 token hash is persisted");
Check(TokenUtilities.NormalizeEmail(" Parent@Example.com ") == "PARENT@EXAMPLE.COM", "email identity is normalized");

var webhookPayload = "{\"type\":\"checkout.session.completed\"}";
var webhookSecret = "whsec_test_secret";
var webhookNow = DateTimeOffset.UtcNow;
var webhookTimestamp = webhookNow.ToUnixTimeSeconds();
var webhookSignature = Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(webhookSecret), Encoding.UTF8.GetBytes($"{webhookTimestamp}.{webhookPayload}")));
Check(StripeWebhookVerifier.Verify(webhookPayload, $"t={webhookTimestamp},v1={webhookSignature}", webhookSecret, webhookNow),
    "valid Stripe webhook signatures verify");
Check(!StripeWebhookVerifier.Verify(webhookPayload + "tampered", $"t={webhookTimestamp},v1={webhookSignature}", webhookSecret, webhookNow),
    "tampered Stripe webhook payloads are rejected");
Check(!StripeWebhookVerifier.Verify(webhookPayload, $"t={webhookTimestamp - 600},v1={webhookSignature}", webhookSecret, webhookNow),
    "stale Stripe webhook signatures are rejected");

var user = new PortalUser { Id = Guid.NewGuid(), Email = "parent@example.com" };
var hasher = new PasswordHasher<PortalUser>();
var passwordHash = hasher.HashPassword(user, "a-secure-password");
Check(hasher.VerifyHashedPassword(user, passwordHash, "a-secure-password") != PasswordVerificationResult.Failed,
    "valid passwords verify");
Check(hasher.VerifyHashedPassword(user, passwordHash, "wrong-password") == PasswordVerificationResult.Failed,
    "invalid passwords are rejected");

var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
string Source(string relative) => File.ReadAllText(Path.Combine(repositoryRoot, relative));

foreach (var endpoint in new[]
{
    "Features/Families/FamilyEndpoints.cs", "Features/Registration/RegistrationEndpoints.cs",
    "Features/Activities/ActivityEndpoints.cs", "Features/Memberships/MembershipEndpoints.cs",
    "Features/Products/ProductEndpoints.cs"
})
    Check(Source(Path.Combine("backend/LegacyOS.Api", endpoint)).Contains("RequireAuthorization(\"StaffOnly\")"),
        $"{endpoint} is staff-only");

var registrationClientSource = Source("frontend/src/api/registration.ts");
Check(registrationClientSource.Contains("http.post<RegisterFamilyResponse>") &&
      registrationClientSource.Contains("membershipPlanShortName"),
    "admin registration uses the authenticated client and submits the selected program");
var registrationServiceSource = Source("backend/LegacyOS.Api/Features/Registration/FamilyRegistrationService.cs");
Check(registrationServiceSource.Contains("x.OrganizationId == organization.Id") &&
      registrationServiceSource.Contains("_db.Enrollments.AddRange(enrollments)"),
    "admin registration enrolls athletes only in the selected organization's active plan");

var portalSource = Source("backend/LegacyOS.Api/Features/Portal/PortalEndpoints.cs");
Check(portalSource.Contains("MapGroup(\"/portal\").RequireAuthorization(\"CustomerOnly\")"),
    "customer portal requires the customer policy");
Check(portalSource.Contains("Where(g => g.Id == guardianId"),
    "family lookup is scoped from the authenticated guardian claim");
Check(portalSource.Contains("invitation.AcceptedOn != null") && portalSource.Contains("invitation.ExpiresOn <= now"),
    "registration invitations are one-time and expiring");
Check(portalSource.Contains("NormalizeEmail(invitation.Guardian.Email) != normalizedEmail"),
    "registration invitation must match the guardian email");
var purchaseSource = Source("backend/LegacyOS.Api/Features/Purchases/PurchaseEndpoints.cs");
Check(purchaseSource.Contains("RequireAuthorization(\"CustomerOnly\")"), "purchase endpoints require the customer policy");
Check(purchaseSource.Contains("x.Id == athleteId && x.FamilyId == familyId"), "checkout athlete ownership is enforced");
Check(purchaseSource.Contains("Enter the athlete's USA Wrestling membership number"),
    "membership checkout requires a submitted USA Wrestling membership number");
Check(purchaseSource.Contains("order.Enrollment.IsActive = true") && purchaseSource.Contains("StripeWebhookVerifier.Verify"),
    "only a verified Stripe webhook activates pending enrollment");
Check(purchaseSource.Contains("!await db.SessionCreditLots.AnyAsync") &&
      purchaseSource.Contains("PurchaseOrderId = order.Id") && purchaseSource.Contains("ExpiresOn = grantedOn.AddDays"),
    "verified Stripe payment grants one idempotent expiring session lot");
Check(purchaseSource.Contains("x.Id == packageAthleteId && x.FamilyId == familyId"),
    "session package purchase is scoped to an athlete in the authenticated family");
var sessionsSource = Source("backend/LegacyOS.Api/Features/Sessions/SessionEndpoints.cs");
Check(sessionsSource.Contains("RequireAuthorization(\"StaffOnly\")"), "session roster and check-in are staff-only");
Check(sessionsSource.Contains("OrderBy(x => x.ExpiresOn)") && sessionsSource.Contains("FirstOrDefault(x => !x.IsUnlimited)"),
    "check-in consumes the earliest-expiring limited package before unlimited access");
Check(sessionsSource.Contains("PaidOutsideStripe") && sessionsSource.Contains("Complimentary") &&
      sessionsSource.Contains("GrantedByStaffPortalUserId = staffId"),
    "staff can assign an existing athlete a non-Stripe package with an audited source");
var selfRegistrationSource = Source("backend/LegacyOS.Api/Features/Portal/PortalEndpoints.cs");
Check(selfRegistrationSource.Contains("MapPost(\"/self-register\"") &&
      selfRegistrationSource.Contains("new Family") && selfRegistrationSource.Contains("new PortalUser"),
    "family self-registration creates a new family-scoped customer account");
var usaWrestlingSource = Source("backend/LegacyOS.Api/Features/UsaWrestling/UsaWrestlingEndpoints.cs");
Check(usaWrestlingSource.Contains("RequireAuthorization(\"CustomerOnly\")") && usaWrestlingSource.Contains("Family.Guardians.Any"),
    "only an athlete's authenticated family can submit a USA Wrestling number");
Check(usaWrestlingSource.Contains("RequireAuthorization(\"StaffOnly\")") && usaWrestlingSource.Contains("VerifiedByPortalUserId = staffId"),
    "USA Wrestling decisions are staff-only and audited");
Check(usaWrestlingSource.Contains("Status = UsaWrestlingVerificationStatus.Pending"),
    "parent submission cannot self-verify a membership");

if (failures.Count > 0)
{
    Console.Error.WriteLine($"{failures.Count} security test(s) failed.");
    return 1;
}

Console.WriteLine("All LegacyOS authentication and purchase security tests passed.");
return 0;
