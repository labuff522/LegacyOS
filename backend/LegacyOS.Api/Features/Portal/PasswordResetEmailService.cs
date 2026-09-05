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
        await SendCoreAsync(email, "Reset your DenOS password",
            $"<p>A password reset was requested for your DenOS account.</p><p><a href=\"{HtmlEncoder.Default.Encode(resetUrl)}\">Reset your password</a></p><p>This link expires in 30 minutes and can be used once.</p>", apiKey, from, ct);
    }

    public async Task SendInvitationAsync(string email, string rawToken, CancellationToken ct)
    {
        var apiKey = configuration["Email:ResendApiKey"];
        var from = configuration["Email:From"];
        var frontend = configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(frontend))
            throw new InvalidOperationException("Invitation email is not configured.");
        var invitationUrl = $"{frontend}/portal/register?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(email)}";
        await SendCoreAsync(email, "Create your DenOS family account",
            $"<p>Your family has been added to DenOS.</p><p><a href=\"{HtmlEncoder.Default.Encode(invitationUrl)}\">Create your password and access your family account</a></p><p>This invitation expires in 48 hours and can be used once.</p>", apiKey, from, ct);
    }

    private async Task SendCoreAsync(string email, string subject, string html, string apiKey, string from, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new { from, to = new[] { email }, subject, html });
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
