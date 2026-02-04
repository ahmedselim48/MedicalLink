using MedLink.Domain.Common;
using MedLink.Domain.Entities.Medical;
using MedLink.Domain.Entities.Payments;
using MedLink.Domain.Enums;
using MedLink.Domain.Identity;

namespace MedLink.Domain.Entities.Appointments;

public class Appointment : BaseEntity
{

    // public string UserId { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int ScheduleId { get; set; }
    public DoctorAvailability Schedule { get; set; } = null!;


    public string PatientName { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string? PatientEmail { get; set; }
    public string? Notes { get; set; }


    public string BookedByUserId { get; set; } = string.Empty;
    public ApplicationUser BookedByUser { get; set; } = null!;


    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public decimal Fee { get; set; }


    public string? CancelledReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Payment? Payment { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
