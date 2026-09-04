using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;

namespace LegacyOS.Api.Features.Portal;

public static class TokenUtilities
{
    public static string CreateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public static string Hash(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && MailAddress.TryCreate(email.Trim(), out var address) &&
        string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
}
