using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Payments
{
    public class CreatePaymentRequest
    {
        [Required]
        public int AppointmentId { get; set; }

        public string? CustomerEmail { get; set; }

        public string? SuccessUrl { get; set; }

        public string? CancelUrl { get; set; }
    }
}
