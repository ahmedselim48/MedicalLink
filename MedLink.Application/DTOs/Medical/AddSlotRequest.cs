using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Medical
{
    public class AddSlotRequest
    {
        public int? DoctorId { get; set; }
        
        public DateTime Date { get; set; }
        
        [Required]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time format must be HH:mm")]
        public string StartTime { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time format must be HH:mm")]
        public string EndTime { get; set; } = string.Empty;
    }
}
