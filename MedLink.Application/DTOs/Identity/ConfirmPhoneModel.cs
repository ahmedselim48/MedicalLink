using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Identity
{
    public class ConfirmPhoneModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
