
namespace MedLink.Application.DTOs.Appointments
{
    public class AppointmentDto
    {
        public int Id { get; set; }

        // Doctor Info
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? DoctorSpecialization { get; set; }

        // Schedule Info
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Patient Info
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }
        public string? Notes { get; set; }

        // Booking Info
        public string BookedByUserId { get; set; } = string.Empty;

        // Status & Fee
        public string Status { get; set; } = string.Empty;
        public decimal Fee { get; set; }

        // Cancellation
        public string? CancelledReason { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Payment Info
        public int? PaymentId { get; set; }
        public string? PaymentStatus { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
