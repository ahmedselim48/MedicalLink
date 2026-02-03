namespace MedLink.Application.DTOs.Appointments
{
    public class CreateAppointmentRequest
    {
        public int ScheduleId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }
        public string? Notes { get; set; }
        public string BookedByUserId { get; set; } = string.Empty;
    }
}
