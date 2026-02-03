using MedLink.Application.DTOs.Medical;

namespace MedLink.Application.DTOs.Doctors
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SpecialtyId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string City { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal ConsultationFee { get; set; }
        public List<DoctorAvailabilityDto> Availabilities { get; set; } = new();
    }
}
