namespace PaymentAPI.Models;

public class PaymentResponse
{
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; 
    public string PaymentId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty; 
    public string ClientSecret { get; set; } = string.Empty; 
    public string SessionId { get; set; } = string.Empty; // Add session ID for Stripe
}
