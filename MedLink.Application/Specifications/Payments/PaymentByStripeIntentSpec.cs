using MedLink.Domain.Entities.Payments;

namespace MedLink.Application.Specifications.Payments
{
    /// <summary>
    /// Specification to get a payment by Stripe PaymentIntent ID or Checkout Session ID
    /// </summary>
    public class PaymentByStripeIntentSpec : BaseSpecification<Payment>
    {
        public PaymentByStripeIntentSpec(string paymentIntentId)
            : base(p => (p.StripePaymentIntentId == paymentIntentId || p.CheckoutSessionId == paymentIntentId) && !p.IsDeleted)
        {
            AddIncludes(p => p.Appointment);
        }
    }
}
