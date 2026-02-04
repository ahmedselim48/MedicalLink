using MedLink.Application.DTOs.Payments;
using MedLink.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace MedLink.Application.Services
{

    public class StripeService : IStripeService
    {
        private readonly string _secretKey;

        public StripeService(IConfiguration configuration)
        {
            _secretKey = configuration["Stripe:SecretKey"]
                ?? throw new ArgumentNullException("Stripe:SecretKey is not configured");

            StripeConfiguration.ApiKey = _secretKey;
        }



        public async Task<(string SessionId, string CheckoutUrl)> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string customerEmail,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string>? metadata = null)
        {
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                CustomerEmail = customerEmail,
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            Currency = currency.ToLower(),
                            UnitAmount = (long)(amount * 100),
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Medical Appointment Payment",
                                Description = "Payment for doctor consultation"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var sessionService = new Stripe.Checkout.SessionService();
            var session = await sessionService.CreateAsync(options);

            return (session.Id, session.Url);
        }



        public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                await service.CancelAsync(paymentIntentId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null, string? reason = null)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Reason = reason switch
                    {
                        _ when reason?.Contains("duplicate") == true => "duplicate",
                        _ when reason?.Contains("fraud") == true => "fraudulent",
                        _ => "requested_by_customer"
                    }
                };

                if (amount.HasValue)
                {
                    options.Amount = (long)(amount.Value * 100);
                }

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                return refund.Status == StripeConstants.StatusSucceeded || refund.Status == "pending"; // pending doesn't have a const yet
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetPaymentStatusAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);
                return paymentIntent.Status;
            }
            catch
            {
                return "unknown";
            }
        }
        public async Task<string?> GetPaymentIntentIdBySessionIdAsync(string sessionId)
        {
             try
            {
                var service = new Stripe.Checkout.SessionService();
                var session = await service.GetAsync(sessionId);
                
                // If the session is complete, it should have a PaymentIntentId
                return session?.PaymentIntentId;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetPaymentStatusBySessionIdAsync(string sessionId)
        {
            try
            {
                var service = new Stripe.Checkout.SessionService();
                var session = await service.GetAsync(sessionId);
                return session?.PaymentStatus ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }

}
