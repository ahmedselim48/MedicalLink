using MedLink.Application.DTOs.Payments;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Medical_Team_B.Controllers
{
    [Authorize]
    /// <summary>
    /// Manages payment processing and retrieval.
    /// </summary>
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Creates a payment for an appointment.
        /// </summary>
        /// <param name="request">The payment creation request details.</param>
        /// <remarks>
        /// When UseCheckoutSession is true (default), returns a CheckoutPaymentResponse with checkoutUrl.
        /// When UseCheckoutSession is false, returns an EmbeddedPaymentResponse with clientSecret.
        /// </remarks>
        /// <response code="200">Payment created successfully.</response>
        /// <response code="400">Invalid request or payment already exists.</response>
        /// <response code="404">Appointment not found.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CheckoutPaymentResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var payment = await _paymentService.CreatePaymentAsync(request);

            return Ok(new CheckoutPaymentResponse
            {
                PaymentId = payment.Id,
                CheckoutUrl = payment.CheckoutUrl ?? "",
                CheckoutSessionId = payment.CheckoutSessionId ?? "",
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                Message = "Redirect user to CheckoutUrl to complete payment"
            });
        }



        /// <summary>
        /// Confirms a payment after it's been completed (Admin/Internal use only).
        /// </summary>
        /// <param name="request">The payment confirmation details.</param>
        /// <remarks>
        /// This endpoint should normally only be called by webhooks.
        /// Manual confirmation requires Admin role.
        /// </remarks>
        [HttpPost("confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            var success = await _paymentService.ConfirmPaymentByAppointmentIdAsync(request.AppointmentId);
            if (!success) return NotFound(new { error = "Payment for this appointment not found" });

            return Ok(new { message = "Payment confirmed successfully" });
        }

        /// <summary>
        /// Gets payment details by appointment ID.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment.</param>
        [HttpGet("appointment/{appointmentId}")]
        public async Task<ActionResult<PaymentDto>> GetPaymentByAppointment(int appointmentId)
        {
            var payment = await _paymentService.GetPaymentByAppointmentIdAsync(appointmentId);
            return Ok(payment);
        }

        /// <summary>
        /// Gets all payments for the current authenticated user.
        /// </summary>
        [HttpGet("my-payments")]
        public async Task<ActionResult<List<PaymentDto>>> GetMyPayments()
        {
            var payments = await _paymentService.GetMyPaymentsAsync(UserId);
            return Ok(payments);
        }


    }
}
