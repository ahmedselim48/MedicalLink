using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Medical
{
    public class AddWeekScheduleRequest
    {
        public int? DoctorId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }

        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public int SlotDurationMinutes { get; set; } = 30;

        public List<DayOfWeek> WorkDays { get; set; } = new List<DayOfWeek> 
        { 
            DayOfWeek.Sunday, 
            DayOfWeek.Monday, 
            DayOfWeek.Tuesday, 
            DayOfWeek.Wednesday, 
            DayOfWeek.Thursday 
        };
    }
}
