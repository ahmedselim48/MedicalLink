using MedLink.Domain.Common;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities.Payments;

public class Payment : BaseEntity
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public string? StripePaymentIntentId { get; set; }
    public string? CheckoutSessionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? RefundReason { get; set; }
    public bool IsDeleted { get; set; }
    public string? FailureReason { get; set; }
    public string? StripeClientSecret { get; set; }
    public string? CheckoutUrl { get; set; }
    public PaymentMethod Method { get; set; }
}
