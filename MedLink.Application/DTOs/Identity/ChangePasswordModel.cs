using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Identity
{
    public class ChangePasswordModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }
}
