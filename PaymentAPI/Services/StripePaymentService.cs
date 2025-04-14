using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentAPI.Exceptions;
using PaymentAPI.Models;
using Stripe;
using Stripe.Checkout;

namespace PaymentAPI.Services;

public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _domain;

    public StripePaymentService(IOptions<StripeSettings> settings, IOptions<AppSettings> appSettings, ILogger<StripePaymentService> logger)
    {
        StripeConfiguration.ApiKey = settings.Value.SecretKey;
        _domain = appSettings.Value.FrontendUrl ?? "http://localhost:8082";
        _logger = logger;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing Stripe payment with Checkout: {Amount} {Currency}", 
                request.Amount, request.Currency);
            
            // Ensure we have valid data before making the request
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero");
            }

            // Log more detailed information for troubleshooting
            _logger.LogInformation("Creating Stripe session with domain: {Domain}, Email: {Email}", 
                _domain, !string.IsNullOrEmpty(request.CustomerEmail) ? "Provided" : "Not provided");
            
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = request.Currency.ToLower(),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Mosselsouper Reservatie",
                                Description = request.Description ?? "Reservatie"
                            },
                            UnitAmount = (long)(request.Amount * 100), // Convert to cents
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{_domain}/confirmation?session_id={{CHECKOUT_SESSION_ID}}&status=success&provider=stripe",
                CancelUrl = $"{_domain}/confirmation?session_id={{CHECKOUT_SESSION_ID}}&status=cancel&provider=stripe",
            };
            
            // Only add email if provided to avoid potential validation errors
            if (!string.IsNullOrEmpty(request.CustomerEmail))
            {
                options.CustomerEmail = request.CustomerEmail;
            }

            var service = new SessionService();
            
            _logger.LogInformation("About to call Stripe API to create session");
            var session = await service.CreateAsync(options);
            _logger.LogInformation("Stripe Checkout Session created successfully: {SessionId} with URL: {Url}", 
                session.Id, session.Url);

            return new PaymentResponse
            {
                Provider = "Stripe",
                Status = "pending",
                PaymentId = session.Id,
                CheckoutUrl = session.Url, // Stripe Checkout URL
                SessionId = session.Id
            };
        }
        catch (StripeException se)
        {
            _logger.LogError(se, "Stripe API error: {Message}, Code: {Code}", se.Message, se.StripeError?.Code);
            throw new PaymentException($"Stripe API error: {se.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe payment");
            throw new PaymentException($"Stripe payment failed: {ex.Message}");
        }
    }
}