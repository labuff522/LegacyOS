using System.Security.Cryptography;
using System.Text;

namespace LegacyOS.Api.Features.Purchases;

public static class StripeWebhookVerifier
{
    public static bool Verify(string payload, string signatureHeader, string secret, DateTimeOffset now)
    {
        long? timestamp = null; var signatures = new List<string>();
        foreach (var part in signatureHeader.Split(','))
        {
            var pair = part.Split('=', 2);
            if (pair.Length != 2) continue;
            if (pair[0] == "t" && long.TryParse(pair[1], out var parsed)) timestamp = parsed;
            if (pair[0] == "v1") signatures.Add(pair[1]);
        }
        if (timestamp is null || Math.Abs(now.ToUnixTimeSeconds() - timestamp.Value) > 300) return false;
        var expected = Convert.ToHexStringLower(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{payload}")));
        return signatures.Any(value => value.Length == expected.Length &&
            CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(value), Encoding.ASCII.GetBytes(expected)));
    }
}
