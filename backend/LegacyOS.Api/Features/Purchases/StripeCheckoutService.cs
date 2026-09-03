using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LegacyOS.Api.Features.Purchases;

public class StripeCheckoutService(HttpClient client, IConfiguration configuration)
{
    public async Task<StripeCheckoutResult> CreateAsync(PurchaseOrder order, string itemName, string email, CancellationToken ct)
    {
        var secret = configuration["Stripe:SecretKey"];
        var frontendUrl = configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(secret) || secret.Contains("REPLACE_ME", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(frontendUrl))
            throw new InvalidOperationException("Stripe checkout is not configured. Add a Stripe test secret key and Frontend:BaseUrl, then restart the API.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/checkout/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secret}:")));
        var values = new Dictionary<string, string>
        {
            ["mode"] = order.Kind == PurchaseKind.MembershipPlan ? "subscription" : "payment",
            ["success_url"] = $"{frontendUrl}/portal/purchase/success?session_id={{CHECKOUT_SESSION_ID}}",
            ["cancel_url"] = $"{frontendUrl}/portal?checkout=cancelled",
            ["customer_email"] = email,
            ["client_reference_id"] = order.Id.ToString(),
            ["metadata[purchase_order_id]"] = order.Id.ToString(),
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = order.Currency,
            ["line_items[0][price_data][unit_amount]"] = decimal.Round(order.Amount * 100m).ToString("0"),
            ["line_items[0][price_data][product_data][name]"] = itemName
        };
        if (order.Kind == PurchaseKind.MembershipPlan)
            values["line_items[0][price_data][recurring][interval]"] = "month";
        request.Content = new FormUrlEncodedContent(values);

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Stripe rejected Checkout Session creation ({(int)response.StatusCode}).");
        using var json = JsonDocument.Parse(body);
        return new(json.RootElement.GetProperty("id").GetString()!, json.RootElement.GetProperty("url").GetString()!);
    }
}

public record StripeCheckoutResult(string SessionId, string Url);
