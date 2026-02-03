using System.ComponentModel.DataAnnotations;
using MedLink.Domain.Common;
using MedLink.Domain.Entities.Medical;

namespace MedLink.Domain.Entities.Appointments;

public class DoctorAvailability : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public bool IsBooked { get; set; } = false;

    // Navigation
    public Appointment? Appointment { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Domain Logic
    public bool IsPast(DateTime currentTime)
    {
        var slotTime = Date.Add(StartTime);
        return slotTime <= currentTime;
    }

    /// <summary>
    /// Attempts to book the slot. Throws exception if validation fails.
    /// </summary>
    public void Book(DateTime currentTime)
    {
        if (IsBooked)
            throw new InvalidOperationException("This time slot is already booked");

        if (IsPast(currentTime))
            throw new InvalidOperationException("Cannot book appointments in the past");

        IsBooked = true;
        UpdatedAt = currentTime;
    }

    public void Release(DateTime currentTime)
    {
        if (!IsBooked) return; // Idempotent

        IsBooked = false;
        UpdatedAt = currentTime;
    }
}
