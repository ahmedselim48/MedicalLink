namespace MedLink.Application.DTOs.Medical
{
    public class AddDayScheduleRequest
    {
        public int? DoctorId { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;      // "10:00"
        public string EndTime { get; set; } = string.Empty;        // "17:00"
        public int? SlotDurationMinutes { get; set; } = 15;        // Default: 15 minutes
    }
}
