

namespace MedLink.Domain.Interfaces.Repositories
{
    public interface IStripeService
    {
        /// <summary>
        /// Creates a Stripe Checkout Session and returns the session ID and checkout URL
        /// </summary>
        Task<(string SessionId, string CheckoutUrl)> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string customerEmail,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string>? metadata = null);

        Task<bool> CancelPaymentIntentAsync(string paymentIntentId);

        Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null, string? reason = null);

        Task<string> GetPaymentStatusAsync(string paymentIntentId);
        
        Task<string?> GetPaymentIntentIdBySessionIdAsync(string sessionId);

        Task<string> GetPaymentStatusBySessionIdAsync(string sessionId);
    }
}
