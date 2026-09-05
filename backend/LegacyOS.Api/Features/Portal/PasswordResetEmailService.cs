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
        var invitationUrl = $"{frontend}/portal/accept-invitation?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(email)}";
        await SendCoreAsync(email, "Create your DenOS family account",
            $"<p>Your family has been added to DenOS.</p><p><a href=\"{HtmlEncoder.Default.Encode(invitationUrl)}\">Create your password and access your family account</a></p><p>This invitation expires in 48 hours and can be used once.</p>", apiKey, from, ct);
    }

    public async Task SendTestAsync(string email, CancellationToken ct)
    {
        var apiKey = configuration["Email:ResendApiKey"];
        var from = configuration["Email:From"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Email__ResendApiKey or Email__From is missing in the hosted API environment.");
        await SendCoreAsync(email, "DenOS email delivery test",
            "<p>Your DenOS email configuration is working.</p>", apiKey, from, ct);
    }

    public async Task SendPurchaseConfirmationAsync(string email, string itemName, string? athleteName,
        decimal amount, string currency, Guid orderId, CancellationToken ct)
    {
        var apiKey = configuration["Email:ResendApiKey"];
        var from = configuration["Email:From"];
        var frontend = configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(frontend))
            throw new InvalidOperationException("Purchase confirmation email is not configured.");
        var encoder = HtmlEncoder.Default;
        var logoUrl = encoder.Encode($"{frontend}/the-den-wrestling-center-logo.png");
        var athleteLine = string.IsNullOrWhiteSpace(athleteName) ? "" : $"<p><strong>Athlete:</strong> {encoder.Encode(athleteName)}</p>";
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;color:#171717">
              <div style="background:#050505;padding:24px;text-align:center"><img src="{logoUrl}" alt="The Den Wrestling Center" width="150" style="max-width:150px;height:auto" /></div>
              <div style="padding:28px;border:1px solid #e5e5e5"><h1 style="font-size:24px">Thank you for your purchase!</h1>
              <p>Your purchase from The Den Wrestling Center was successful.</p><p><strong>Purchase:</strong> {encoder.Encode(itemName)}</p>{athleteLine}
              <p><strong>Total:</strong> {amount:0.00} {encoder.Encode(currency.ToUpperInvariant())}</p>
              <p><strong>Order:</strong> {orderId}</p><p>You can review your athlete and current package in the <a href="{encoder.Encode(frontend + "/portal")}">Family Portal</a>.</p></div>
            </div>
            """;
        await SendCoreAsync(email, $"Thank you for your purchase — {itemName}", html, apiKey, from, ct,
            $"purchase-confirmation/{orderId:N}");
    }

    private async Task SendCoreAsync(string email, string subject, string html, string apiKey, string from,
        CancellationToken ct, string? idempotencyKey = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        if (idempotencyKey is not null) request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        request.Content = JsonContent.Create(new { from, to = new[] { email }, subject, html });
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(ct);
            if (detail.Length > 1000) detail = detail[..1000];
            throw new InvalidOperationException($"Resend rejected the email ({(int)response.StatusCode}): {detail}");
        }
    }
}
