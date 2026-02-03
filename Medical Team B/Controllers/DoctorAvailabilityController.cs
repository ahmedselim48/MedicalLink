using MedLink.Application.DTOs.Appointments;
using MedLink.Application.DTOs.Medical;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    [Authorize(Roles = "Doctor,Admin")] 
    public class DoctorAvailabilityController : BaseApiController
    {
        private readonly IDoctorAvailabilityService _availabilityService;

        public DoctorAvailabilityController(IDoctorAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        /// <summary>
        /// Adds a single time slot for a doctor.
        /// </summary>
        [HttpPost("single-slot")]
        public async Task<ActionResult<DoctorAvailabilityDto>> AddSingleSlot([FromBody] AddSlotRequest request)
        {
            var slot = await _availabilityService.AddSingleSlotAsync(request, UserId);
            return Ok(slot);
        }

        /// <summary>
        /// Generates slots for a full day based on start/end time and duration.
        /// </summary>
        [HttpPost("day-schedule")]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> AddDaySchedule([FromBody] AddDayScheduleRequest request)
        {
            var slots = await _availabilityService.AddDayScheduleAsync(request, UserId);
            return Ok(slots);
        }

        /// <summary>
        /// Generates slots for a full week (starting from StartDate).
        /// </summary>
        [HttpPost("week-schedule")]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> AddWeekSchedule([FromBody] AddWeekScheduleRequest request)
        {
            var slots = await _availabilityService.AddWeekScheduleAsync(request, UserId);
            return Ok(slots);
        }

        /// <summary>
        /// Deletes a specific time slot.
        /// </summary>
        [HttpDelete("{slotId}")]
        public async Task<IActionResult> DeleteSlot(int slotId)
        {
            await _availabilityService.DeleteSlotAsync(slotId);
            return NoContent();
        }

        /// <summary>
        /// Gets all slots (booked and unbooked) for a doctor.
        /// </summary>
        [HttpGet("doctor/{doctorId}/all")]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> GetAllDoctorSlots(int doctorId)
        {
            var slots = await _availabilityService.GetAllDoctorSlotsAsync(doctorId);
            return Ok(slots);
        }

        /// <summary>
        /// Gets only available slots for a doctor.
        /// </summary>
        [HttpGet("doctor/{doctorId}/available")]
        [AllowAnonymous]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> GetAvailableSlots(int doctorId, [FromQuery] DateTime? date)
        {
            var slots = await _availabilityService.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }


    }
}
