using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;

namespace LegacyOS.Api.Features.Portal;

public sealed class PasswordResetEmailService(HttpClient http, IConfiguration configuration)
{
    public async Task SendAsync(string email, string rawToken, CancellationToken ct)
    {
        var apiKey = configuration["Email:ResendApiKey"];
        var from = configuration["Email:From"];
        var frontend = configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(frontend))
            throw new InvalidOperationException("Password recovery email is not configured.");
        var resetUrl = $"{frontend}/portal/reset-password?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(email)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new { from, to = new[] { email }, subject = "Reset your DenOS password",
            html = $"<p>A password reset was requested for your DenOS account.</p><p><a href=\"{HtmlEncoder.Default.Encode(resetUrl)}\">Reset your password</a></p><p>This link expires in 30 minutes and can be used once.</p>" });
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
