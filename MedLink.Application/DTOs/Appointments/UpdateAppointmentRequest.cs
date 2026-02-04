
namespace MedLink.Application.DTOs.Appointments
{
    public class UpdateAppointmentRequest
    {
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }
        public string? Notes { get; set; }
        public string UpdatedByUserId { get; set; } = string.Empty;
    }
}
