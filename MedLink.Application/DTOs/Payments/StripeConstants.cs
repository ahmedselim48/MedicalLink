namespace MedLink.Application.DTOs.Payments;

public static class StripeConstants
{
    public const string PaymentStatusPaid = "paid";
    public const string PaymentStatusNoPaymentRequired = "no_payment_required";
    public const string StatusSucceeded = "succeeded";
    public const string StatusCanceled = "canceled";
    public const string StatusFailed = "failed";
    public const string StatusExpired = "expired";
    public const string StatusProcessing = "processing";
    public const string StatusRequiresPaymentMethod = "requires_payment_method";
    public const string StatusRequiresAction = "requires_action";
    public const string DefaultCurrency = "usd";
}
