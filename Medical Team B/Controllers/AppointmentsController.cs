using MedLink.Application.DTOs.Appointments;
using MedLink.Application.DTOs.Medical;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Medical_Team_B.Controllers
{
    /// <summary>
    /// Manages appointment operations.
    /// </summary>
    [Authorize]
    public class AppointmentsController : BaseApiController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        /// <summary>
        /// Retrieves available time slots for a doctor.
        /// </summary>
        /// <param name="doctorId">The ID of the doctor.</param>
        /// <param name="date">Optional date to filter slots.</param>
        [HttpGet("available-slots")]
        [AllowAnonymous]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> GetAvailableSlots(
            [FromQuery] int doctorId,
            [FromQuery] DateTime? date)
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }

        /// <summary>
        /// Creates a new appointment.
        /// </summary>
        /// <param name="request">The appointment creation request.</param>
        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            request.BookedByUserId = UserId;

            var appointment = await _appointmentService.CreateAppointmentAsync(request);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, appointment);
        }

        /// <summary>
        /// Retrieves appointment details by ID.
        /// </summary>
        /// <param name="id">The ID of the appointment.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointmentById(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            return Ok(appointment);
        }

        /// <summary>
        /// Updates an existing appointment.
        /// </summary>
        /// <param name="id">The ID of the appointment to update.</param>
        /// <param name="request">The update request details.</param>
        [HttpPut("{id}")]
        public async Task<ActionResult<AppointmentDto>> UpdateAppointment(
            int id,
            [FromBody] UpdateAppointmentRequest request)
        {
            // Set user ID strictly from token for security
            request.UpdatedByUserId = UserId;

            var appointment = await _appointmentService.UpdateAppointmentAsync(id, request);
            return Ok(appointment);
        }

        /// <summary>
        /// Retrieves appointments for a specific doctor.
        /// </summary>
        /// <param name="doctorId">The ID of the doctor.</param>
        /// <param name="date">Optional date to filter appointments.</param>
        [HttpGet("doctor/{doctorId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AppointmentDto>>> GetAppointmentsByDoctor(
            int doctorId,
            [FromQuery] DateTime? date = null)
        {
            var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId, date);
            return Ok(appointments);
        }

        /// <summary>
        /// Retrieves appointments for the current user.
        /// </summary>
        [HttpGet("my-appointments")]
        public async Task<ActionResult<List<AppointmentDto>>> GetMyAppointments()
        {
            var appointments = await _appointmentService.GetMyAppointmentsAsync(UserId);
            return Ok(appointments);
        }

        /// <summary>
        /// Retrieves upcoming appointments for the current user.
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<List<AppointmentDto>>> GetUpcomingAppointments()
        {
            var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(UserId);
            return Ok(appointments);
        }


        /// <summary>
        /// Reschedules an existing appointment.
        /// </summary>
        /// <param name="id">The ID of the appointment.</param>
        /// <param name="request">The reschedule request details.</param>
        [HttpPost("{id}/reschedule")]
        public async Task<ActionResult<AppointmentDto>> RescheduleAppointment(
            int id,
            [FromBody] RescheduleAppointmentRequest request)
        {
            var appointment = await _appointmentService.RescheduleAppointmentAsync(id, request.NewScheduleId, UserId);
            return Ok(appointment);
        }

        /// <summary>
        /// Cancels an appointment.
        /// </summary>
        /// <param name="id">The ID of the appointment.</param>
        /// <param name="request">The cancellation request details.</param>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentRequest request)
        {
            await _appointmentService.CancelAppointmentAsync(id, request.Reason, UserId);
            return Ok(new { message = "Appointment cancelled successfully" });
        }

        /// <summary>
        /// Marks an appointment as completed.
        /// </summary>
        /// <param name="id">The ID of the appointment.</param>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteAppointment(int id)
        {
            await _appointmentService.CompleteAppointmentAsync(id, UserId);
            return Ok(new { message = "Appointment completed successfully" });
        }

    }
}
