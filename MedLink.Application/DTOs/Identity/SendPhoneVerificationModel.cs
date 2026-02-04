using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Identity
{
    public class SendPhoneVerificationModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }
    }
}
