using AutoMapper;
using MedLink.Application.DTOs.Payments;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications.Payments;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Payments;
using MedLink.Domain.Enums;
using MedLink.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace MedLink.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStripeService _stripeService;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStripeService stripeService,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _stripeService = stripeService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request)
        {
            _logger.LogInformation("Creating payment for Appointment {AppointmentId}", request.AppointmentId);

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(request.AppointmentId);
            if (appointment == null)
                throw new KeyNotFoundException($"Appointment {request.AppointmentId} not found");

            if (appointment.Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot pay for a cancelled appointment.");

            // Check if payment already exists
            var existingPaymentSpec = new PaymentByAppointmentSpec(request.AppointmentId);
            var existingPayment = await _unitOfWork.Repository<Payment>().GetEntityWithAsync(existingPaymentSpec);

            if (existingPayment != null && existingPayment.Status == PaymentStatus.Succeeded)
            {
                 _logger.LogWarning("Attempted to create payment for already paid appointment {AppointmentId}", request.AppointmentId);
                 throw new InvalidOperationException("Payment for this appointment already succeeded.");
            }

            var currency = _configuration["Stripe:Currency"]?.ToLower() ?? StripeConstants.DefaultCurrency;

            var payment = existingPayment ?? new Payment
            {
                AppointmentId = request.AppointmentId,
                Amount = appointment.Fee,
                Currency = currency,
                Status = PaymentStatus.Pending,
                Method = MedLink.Domain.Enums.PaymentMethod.Stripe
            };

            var result = await _stripeService.CreateCheckoutSessionAsync(
                payment.Amount,
                payment.Currency,
                request.CustomerEmail ?? string.Empty,
                request.SuccessUrl ?? _configuration["Stripe:SuccessUrl"],
                request.CancelUrl ?? _configuration["Stripe:CancelUrl"],
                new Dictionary<string, string> { { "appointmentId", appointment.Id.ToString() } }
            );

            payment.CheckoutSessionId = result.SessionId;
            payment.CheckoutUrl = result.CheckoutUrl;

            if (existingPayment == null)
                await _unitOfWork.Repository<Payment>().AddAsync(payment);
            else
                _unitOfWork.Repository<Payment>().Update(payment);

            await _unitOfWork.Complete();

            var dto = _mapper.Map<PaymentDto>(payment);
            dto.CheckoutSessionId = payment.CheckoutSessionId;
            dto.CheckoutUrl = payment.CheckoutUrl;

            return dto;
        }


        public async Task<bool> ConfirmPaymentByStripeIdAsync(string stripeId, bool isSession, string? paymentIntentId = null)
        {
            var spec = new PaymentByStripeIntentSpec(stripeId);
            var payment = await _unitOfWork.Repository<Payment>().GetEntityWithAsync(spec);
            return await ProcessPaymentLogic(payment, stripeId, isSession, paymentIntentId);
        }
        public async Task<bool> ConfirmPaymentByAppointmentIdAsync(int appointmentId)
        {
            var spec = new PaymentByAppointmentSpec(appointmentId);
            var payment = await _unitOfWork.Repository<Payment>().GetEntityWithAsync(spec);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for Appointment ID: {AppointmentId}", appointmentId);
                return false;
            }

            //  Try to confirm via Stripe (if SessionId or PaymentIntentId exists)
            if (!string.IsNullOrEmpty(payment.CheckoutSessionId))
            {
                 return await ProcessPaymentLogic(payment, payment.CheckoutSessionId, isSession: true);
            }
            
            if (!string.IsNullOrEmpty(payment.StripePaymentIntentId))
            {
                 return await ProcessPaymentLogic(payment, payment.StripePaymentIntentId, isSession: false);
            }
            
            // 2. Fallback: Manual confirmation (Cash/External)
             return await FinalizePayment(payment);
        }

        private async Task<bool> ProcessPaymentLogic(Payment? payment, string stripeId, bool isSession, string? paymentIntentId = null)
        {
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for Stripe ID: {StripeId}", stripeId);
                return false;
            }

            if (payment.Status == PaymentStatus.Succeeded) 
            {
                _logger.LogInformation("Payment {PaymentId} is already succeeded. Idempotent return.", payment.Id);
                return true;
            }

            string stripeStatus;
            bool isSuccess = false;
            bool isFail = false;

            if (isSession)
            {
                stripeStatus = await _stripeService.GetPaymentStatusBySessionIdAsync(stripeId);
                if (stripeStatus == StripeConstants.PaymentStatusPaid || stripeStatus == StripeConstants.PaymentStatusNoPaymentRequired) isSuccess = true;
                else if (stripeStatus == StripeConstants.StatusExpired || stripeStatus == StripeConstants.StatusCanceled) isFail = true;

                 // Use provided IntentId if available, OR try to fetch if missing
                if (string.IsNullOrEmpty(payment.StripePaymentIntentId))
                {
                    if (!string.IsNullOrEmpty(paymentIntentId))
                    {
                         payment.StripePaymentIntentId = paymentIntentId;
                    }
                    else 
                    {
                        var fetchedIntentId = await _stripeService.GetPaymentIntentIdBySessionIdAsync(stripeId);
                        if (!string.IsNullOrEmpty(fetchedIntentId)) payment.StripePaymentIntentId = fetchedIntentId;
                    }
                }
            }
            else
            {
                stripeStatus = await _stripeService.GetPaymentStatusAsync(stripeId);
                
                if (stripeStatus == StripeConstants.StatusSucceeded) 
                {
                    isSuccess = true;
                }
                else if (stripeStatus == StripeConstants.StatusCanceled || 
                         stripeStatus == StripeConstants.StatusRequiresPaymentMethod) 
                {
                    isFail = true;
                }
                // processing, requires_action, etc. are treated as pending (neither success nor fail)
            }

            // If processing/open, just return false, don't update to failed
            if (!isSuccess && !isFail)
            {
                 _logger.LogInformation("Payment {PaymentId} status is {StripeStatus}, not checking out yet.", payment.Id, stripeStatus);
                return false;
            }

            if (isSuccess)
            {
                return await FinalizePayment(payment);
            }
            
             if (isFail)
             {
                  payment.Status = PaymentStatus.Failed;
                  payment.FailureReason = $"Stripe status: {stripeStatus}";
                  payment.UpdatedAt = DateTime.UtcNow;
                  
                  _unitOfWork.Repository<Payment>().Update(payment);
                  await _unitOfWork.Complete();
                  _logger.LogWarning("Payment {PaymentId} marked as Failed due to Stripe status: {StripeStatus}", payment.Id, stripeStatus);
             }
 
             return false;
         }
 
         private async Task<bool> FinalizePayment(Payment payment)
         {
              // Idempotency check again
              if (payment.Status == PaymentStatus.Succeeded) return true;
 
             await _unitOfWork.BeginTransactionAsync();
             try
             {
                 payment.Status = PaymentStatus.Succeeded;
                 payment.PaidAt = DateTime.UtcNow;
                 payment.UpdatedAt = DateTime.UtcNow;
 
                 _unitOfWork.Repository<Payment>().Update(payment);
 
                 var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(payment.AppointmentId);
                 if (appointment != null)
                 {
                     // Only update if currently pending to avoid overwriting completed/cancelled states
                     if (appointment.Status == AppointmentStatus.Pending)
                     {
                         appointment.Status = AppointmentStatus.Confirmed;
                         appointment.UpdatedAt = DateTime.UtcNow;
                         _unitOfWork.Repository<Appointment>().Update(appointment);
                     }
                     else
                     {
                         _logger.LogWarning("Payment succeeded but Appointment {AppointmentId} status was {Status}. Status left unchanged.", appointment.Id, appointment.Status);
                     }
                 }
 
                 await _unitOfWork.Complete();
                 await _unitOfWork.CommitTransactionAsync();
                 _logger.LogInformation("Payment {PaymentId} confirmed successfully.", payment.Id);
                 return true;
             }
             catch (Exception ex)
             {
                 await _unitOfWork.RollbackTransactionAsync();
                 _logger.LogError(ex, "Failed to finalize payment {PaymentId}", payment.Id);
                 throw;
             }
         }

        public async Task<PaymentDto> GetPaymentByAppointmentIdAsync(int appointmentId)
        {
            var spec = new PaymentByAppointmentSpec(appointmentId);
            var payment = await _unitOfWork.Repository<Payment>().GetEntityWithAsync(spec);

            if (payment == null)
                throw new KeyNotFoundException($"Payment for appointment {appointmentId} not found");

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<List<PaymentDto>> GetMyPaymentsAsync(string userId)
        {
            var spec = new PaymentsByUserSpec(userId);
            var payments = await _unitOfWork.Repository<Payment>().GetAllWithSpecAsync(spec);
            return _mapper.Map<List<PaymentDto>>(payments);
        }
    }
}
