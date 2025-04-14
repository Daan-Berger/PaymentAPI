namespace PaymentAPI.Models;

public class PaymentRequest
{
    private const string euroIsoCode = "EUR";
    public string Provider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = euroIsoCode;
    public string Description { get; set; } = "Default payment";
    public string RedirectUrl { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty; // Add customer email for Stripe
}
