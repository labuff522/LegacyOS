using System.Security.Cryptography;
using System.Text;

namespace LegacyOS.Api.Features.Portal;

public static class TokenUtilities
{
    public static string CreateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public static string Hash(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
