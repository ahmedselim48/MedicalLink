namespace MedLink.Application.DTOs.Payments
{
    /// <summary>
    /// Response for Stripe Checkout Session flow (redirect to Stripe-hosted page)
    /// </summary>
    public class CheckoutPaymentResponse
    {
        public int PaymentId { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
        public string CheckoutSessionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = "Redirect user to CheckoutUrl to complete payment";
    }

    /// <summary>
    /// Response for embedded Stripe Elements flow (client-side payment form)
    /// </summary>
    public class EmbeddedPaymentResponse
    {
        public int PaymentId { get; set; }
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
