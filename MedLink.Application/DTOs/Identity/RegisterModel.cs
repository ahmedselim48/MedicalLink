using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Identity
{
    public class RegisterModel
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; }

        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MaxLength(256)]
        public string Password { get; set; }
    }
}
