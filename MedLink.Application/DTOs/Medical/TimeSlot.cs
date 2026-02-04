namespace MedLink.Application.DTOs.Medical
{
    /// <summary>
    /// فترة زمنية (مثلاً: 9:00 - 9:30)
    /// </summary>
    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
