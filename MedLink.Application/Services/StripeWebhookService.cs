using MedLink.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace MedLink.Application.Services
{
    public interface IStripeWebhookService
    {
        Task HandleStripeEventAsync(string json, string signature);
    }

    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly ILogger<StripeWebhookService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;

        public StripeWebhookService(
            ILogger<StripeWebhookService> logger,
            IConfiguration configuration,
            IPaymentService paymentService)
        {
            _logger = logger;
            _configuration = configuration;
            _paymentService = paymentService;
        }

        public async Task HandleStripeEventAsync(string json, string signature)
        {
            var endpointSecret = _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrEmpty(endpointSecret))
            {
                _logger.LogError("Stripe Webhook Secret is missing in configuration.");
                throw new InvalidOperationException("Webhook secret not configured");
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    endpointSecret
                );

                _logger.LogInformation("Received Stripe Event: {EventType} | ID: {EventId}", stripeEvent.Type, stripeEvent.Id);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                    case "checkout.session.async_payment_failed":
                    case "checkout.session.expired":
                        if (stripeEvent.Data.Object is Session session)
                        {
                            await _paymentService.ConfirmPaymentByStripeIdAsync(session.Id, isSession: true, session.PaymentIntentId);
                        }
                        break;

                    case "payment_intent.succeeded":
                    case "payment_intent.payment_failed":
                    case "payment_intent.canceled":
                        if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                        {
                            await _paymentService.ConfirmPaymentByStripeIdAsync(paymentIntent.Id, isSession: false);
                        }
                        break;

                    default:
                        _logger.LogInformation("Unhandled Stripe Event Type: {EventType}", stripeEvent.Type);
                        break;
                }
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe Webhook Error");
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General Webhook Error");
                throw;
            }
        }
    }
}
