using System.ComponentModel.DataAnnotations;
using MedLink.Domain.Enums;

namespace MedLink.Application.DTOs.Identity
{
    public class AdminCreateDoctorDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public int SpecialtyId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? Bio { get; set; }
        
        [Required]
        public string City { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public string? Address { get; set; }

        public int ConsultationFee { get; set; } = 400;

        // Optional: Location coordinates
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
