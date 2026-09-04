using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LegacyOS.Api.Features.Purchases;

public class StripeCheckoutService(HttpClient client, IConfiguration configuration)
{
    public async Task<StripeCheckoutStatus> GetAsync(string sessionId, CancellationToken ct)
    {
        var secret = configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret)) throw new InvalidOperationException("Stripe checkout is not configured.");
        using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/checkout/sessions/{Uri.EscapeDataString(sessionId)}?expand[]=payment_intent&expand[]=subscription.latest_invoice.payment_intent");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secret}:")));
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Stripe could not confirm this Checkout Session.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = json.RootElement;
        var paymentIntentId = IdFrom(root, "payment_intent");
        if (paymentIntentId is null && root.TryGetProperty("subscription", out var subscriptionObject) && subscriptionObject.ValueKind == JsonValueKind.Object &&
            subscriptionObject.TryGetProperty("latest_invoice", out var invoice) && invoice.ValueKind == JsonValueKind.Object)
            paymentIntentId = IdFrom(invoice, "payment_intent");
        return new StripeCheckoutStatus(root.GetProperty("id").GetString()!, root.GetProperty("payment_status").GetString(),
            root.TryGetProperty("customer", out var customer) && customer.ValueKind == JsonValueKind.String ? customer.GetString() : null,
            IdFrom(root, "subscription"), paymentIntentId);
    }

    public async Task<StripeRefundResult> RefundAndCancelAsync(string? paymentIntentId, string? subscriptionId, Guid orderId, CancellationToken ct)
    {
        var secret = configuration["Stripe:SecretKey"]!; string? refundId = null;
        if (!string.IsNullOrWhiteSpace(paymentIntentId))
        {
            using var refundRequest = new HttpRequestMessage(HttpMethod.Post, "v1/refunds");
            refundRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secret}:")));
            refundRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["payment_intent"] = paymentIntentId, ["reason"] = "requested_by_customer", ["metadata[purchase_order_id]"] = orderId.ToString() });
            using var refundResponse = await client.SendAsync(refundRequest, ct);
            var refundBody = await refundResponse.Content.ReadAsStringAsync(ct);
            if (!refundResponse.IsSuccessStatusCode) throw new InvalidOperationException("Stripe could not refund the collected payment.");
            using var refundJson = JsonDocument.Parse(refundBody); refundId = refundJson.RootElement.GetProperty("id").GetString();
        }
        if (!string.IsNullOrWhiteSpace(subscriptionId))
        {
            using var cancelRequest = new HttpRequestMessage(HttpMethod.Delete, $"v1/subscriptions/{Uri.EscapeDataString(subscriptionId)}");
            cancelRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secret}:")));
            using var cancelResponse = await client.SendAsync(cancelRequest, ct);
            if (!cancelResponse.IsSuccessStatusCode) throw new InvalidOperationException("The payment was refunded, but Stripe could not cancel future installments. Cancel the subscription in Stripe immediately.");
        }
        return new StripeRefundResult(refundId);
    }

    private static string? IdFrom(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ValueKind == JsonValueKind.Object && value.TryGetProperty("id", out var id) ? id.GetString() : null;
    }

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
            ["mode"] = order.InstallmentCount > 1 ? "subscription" : "payment",
            ["success_url"] = $"{frontendUrl}/portal/purchase/success?session_id={{CHECKOUT_SESSION_ID}}",
            ["cancel_url"] = $"{frontendUrl}/portal?checkout=cancelled",
            ["customer_email"] = email,
            ["client_reference_id"] = order.Id.ToString(),
            ["metadata[purchase_order_id]"] = order.Id.ToString(),
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = order.Currency,
            ["line_items[0][price_data][unit_amount]"] = decimal.Round(order.InstallmentAmount * 100m).ToString("0"),
            ["line_items[0][price_data][product_data][name]"] = itemName,
            ["branding_settings[display_name]"] = "DenOS"
        };
        if (order.InstallmentCount > 1)
        {
            values["line_items[0][price_data][recurring][interval]"] = "month";
            var firstCharge = DateTimeOffset.UtcNow;
            if (order.BillingDayOfMonth is int day)
            {
                var now = DateTimeOffset.UtcNow;
                firstCharge = new DateTimeOffset(now.Year, now.Month, day, 12, 0, 0, TimeSpan.Zero);
                if (firstCharge <= now.AddHours(48)) firstCharge = firstCharge.AddMonths(1);
                values["subscription_data[trial_end]"] = firstCharge.ToUnixTimeSeconds().ToString();
            }
        }
        request.Content = new FormUrlEncodedContent(values);

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            var stripeMessage = "Review the product's payment-plan settings.";
            try
            {
                using var errorJson = JsonDocument.Parse(body);
                stripeMessage = errorJson.RootElement.GetProperty("error").GetProperty("message").GetString() ?? stripeMessage;
            }
            catch (JsonException) { }
            throw new InvalidOperationException($"Stripe rejected Checkout Session creation ({(int)response.StatusCode}): {stripeMessage}");
        }
        using var json = JsonDocument.Parse(body);
        return new(json.RootElement.GetProperty("id").GetString()!, json.RootElement.GetProperty("url").GetString()!);
    }

    public async Task SetFiniteEndAsync(PurchaseOrder order, CancellationToken ct)
    {
        if (order.InstallmentCount <= 1 || string.IsNullOrWhiteSpace(order.StripeSubscriptionId)) return;
        var secret = configuration["Stripe:SecretKey"]!;
        var firstCharge = new DateTimeOffset(order.CreatedOn, TimeSpan.Zero);
        if (order.BillingDayOfMonth is int day)
        {
            firstCharge = new DateTimeOffset(firstCharge.Year, firstCharge.Month, day, 12, 0, 0, TimeSpan.Zero);
            if (firstCharge <= order.CreatedOn.AddHours(48)) firstCharge = firstCharge.AddMonths(1);
        }
        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1/subscriptions/{Uri.EscapeDataString(order.StripeSubscriptionId)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secret}:")));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["cancel_at"] = firstCharge.AddMonths(order.InstallmentCount).ToUnixTimeSeconds().ToString() });
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Stripe rejected finite payment-plan scheduling ({(int)response.StatusCode}).");
    }
}

public record StripeCheckoutResult(string SessionId, string Url);
public record StripeCheckoutStatus(string SessionId, string? PaymentStatus, string? CustomerId, string? SubscriptionId, string? PaymentIntentId);
public record StripeRefundResult(string? RefundId);
