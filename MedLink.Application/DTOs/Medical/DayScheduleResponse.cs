namespace MedLink.Application.DTOs.Medical
{
    public class DayScheduleResponse
    {
        public bool Success { get; set; }
        public DateTime Date { get; set; }
        public int TotalSlotsCreated { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<DoctorAvailabilityDto> Slots { get; set; } = new();
    }
}
