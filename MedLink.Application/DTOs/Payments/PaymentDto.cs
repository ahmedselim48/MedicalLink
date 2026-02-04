namespace MedLink.Application.DTOs.Payments
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string? StripeClientSecret { get; set; }
        public string? StripePaymentIntentId { get; set; }

        public string? CheckoutSessionId { get; set; }

        public string? CheckoutUrl { get; set; }

        public string? FailureReason { get; set; }
        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
