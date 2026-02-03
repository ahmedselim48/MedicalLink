using AutoMapper;
using MedLink.Application.DTOs.Appointments;
using MedLink.Application.DTOs.Medical;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications.Appointments;
using MedLink.Application.Specifications.Medical;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private DateTime Now => DateTime.UtcNow;

        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<DoctorAvailabilityDto>> GetAvailableSlotsAsync(int doctorId, DateTime? date)
        {
            var spec = new DoctorAvailabilitySpec(doctorId, date, isAvailable: true);
            var availableSlots = await _unitOfWork.Repository<DoctorAvailability>().GetAllWithSpecAsync(spec);
            return _mapper.Map<List<DoctorAvailabilityDto>>(availableSlots);
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            var spec = new DoctorAvailabilityWithDetailsSpec(request.ScheduleId);
            var schedule = await _unitOfWork.Repository<DoctorAvailability>().GetEntityWithAsync(spec);

            if (schedule == null)
                throw new KeyNotFoundException($"Schedule with ID {request.ScheduleId} not found");

            // check price logic
            decimal fee = schedule.Doctor?.ConsultationFee > 0 ? schedule.Doctor.ConsultationFee : (schedule.Doctor?.Price ?? 0);

            if (fee <= 0)
                throw new InvalidOperationException("Doctor consultation fee is not set. Cannot proceed with booking.");

            if (schedule.IsBooked)
                throw new InvalidOperationException("This time slot is already booked. Please choose another one.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Encapsulated booking logic
                schedule.Book(Now);

                // 2. Map to Entity
                var appointment = new Appointment
                {
                    DoctorId = schedule.DoctorId,
                    ScheduleId = request.ScheduleId,
                    PatientName = request.PatientName,
                    PatientPhone = request.PatientPhone,
                    PatientEmail = request.PatientEmail,
                    Notes = request.Notes,
                    BookedByUserId = request.BookedByUserId,
                    Status = AppointmentStatus.Pending,
                    Fee = fee,
                    CreatedAt = Now
                };

                // 3. Track changes
                _unitOfWork.Repository<DoctorAvailability>().Update(schedule);
                await _unitOfWork.Repository<Appointment>().AddAsync(appointment);

                // 4. Save and Commit
                await _unitOfWork.Complete();
                await _unitOfWork.CommitTransactionAsync();

                // 5. Build Result
                var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
                appointmentDto.Date = schedule.Date;
                appointmentDto.StartTime = schedule.StartTime;
                appointmentDto.EndTime = schedule.EndTime;
                return appointmentDto;
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException("This time slot was just booked by another user. Please choose another slot.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<AppointmentDto> GetAppointmentByIdAsync(int id)
        {
            var spec = new AppointmentWithDetailsSpec(id);
            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityWithAsync(spec);

            if (appointment == null)
                throw new KeyNotFoundException($"Appointment with ID {id} not found");

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<AppointmentDto> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment == null) throw new KeyNotFoundException($"Appointment with ID {id} not found");

            if (appointment.BookedByUserId != request.UpdatedByUserId)
                throw new UnauthorizedAccessException("You are not authorized to update this appointment");

            appointment.PatientName = request.PatientName;
            appointment.PatientPhone = request.PatientPhone;
            appointment.PatientEmail = request.PatientEmail;
            appointment.Notes = request.Notes;
            appointment.UpdatedAt = Now;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.Complete();

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId, DateTime? date)
        {
            var spec = new AppointmentsByDoctorSpec(doctorId, date);
            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(spec);
            return _mapper.Map<List<AppointmentDto>>(appointments);
        }

        public async Task<List<AppointmentDto>> GetMyAppointmentsAsync(string userId)
        {
            var spec = new AppointmentsByUserSpec(userId);
            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(spec);
            return _mapper.Map<List<AppointmentDto>>(appointments);
        }

        public async Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync(string userId)
        {
            var spec = new UpcomingAppointmentsSpec(userId);
            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(spec);
            return _mapper.Map<List<AppointmentDto>>(appointments);
        }

        public async Task<AppointmentDto> RescheduleAppointmentAsync(int id, int newScheduleId, string userId)
        {
            //  Get Appointment WITH Schedule in one go using Spec
            var spec = new AppointmentWithDetailsSpec(id);
            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityWithAsync(spec);

            if (appointment == null) throw new KeyNotFoundException($"Appointment {id} not found");

            if (appointment.BookedByUserId != userId)
                throw new UnauthorizedAccessException("Not authorized");

            // Release Old Schedule (No extra DB call needed)
            if (appointment.Schedule != null)
            {
                appointment.Schedule.Release(Now);
                _unitOfWork.Repository<DoctorAvailability>().Update(appointment.Schedule);
            }

            // Book New Schedule
            // Use Spec to ensure we fetch doctor details if needed and for consistent locking behavior
            var newScheduleSpec = new DoctorAvailabilityWithDetailsSpec(newScheduleId);
            var newSchedule = await _unitOfWork.Repository<DoctorAvailability>().GetEntityWithAsync(newScheduleSpec);

            if (newSchedule == null) throw new KeyNotFoundException("New schedule not found");

            // Validation: Ensure rescheduling keeps the same doctor
            if (newSchedule.DoctorId != appointment.DoctorId)
                throw new InvalidOperationException("Cannot reschedule to a slot with a different doctor.");

            newSchedule.Book(Now);
            _unitOfWork.Repository<DoctorAvailability>().Update(newSchedule);

            // Update Appointment
            appointment.ScheduleId = newScheduleId;
            appointment.UpdatedAt = Now;
            _unitOfWork.Repository<Appointment>().Update(appointment);

            try
             {
                 await _unitOfWork.Complete();
             }
             catch (DbUpdateConcurrencyException)
             {
                 throw new InvalidOperationException("The selected slot was taken while you were rescheduling. Please try again.");
             }

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task CancelAppointmentAsync(int id, string reason, string userId)
        {
            // Use spec to include Schedule efficiently
            var spec = new AppointmentWithDetailsSpec(id);
            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityWithAsync(spec);

            if (appointment == null) throw new KeyNotFoundException($"Appointment {id} not found");

            if (appointment.BookedByUserId != userId)
                throw new UnauthorizedAccessException("Not authorized");

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelledReason = reason;
            appointment.CancelledAt = Now;

            // Free the slot using loaded navigation property
            if (appointment.Schedule != null)
            {
                appointment.Schedule.Release(Now);
                _unitOfWork.Repository<DoctorAvailability>().Update(appointment.Schedule);
            }

            _unitOfWork.Repository<Appointment>().Update(appointment);

            try
             {
                 await _unitOfWork.Complete();
             }
             catch (DbUpdateConcurrencyException)
             {
                 throw new InvalidOperationException("An error occurred while cancelling. Please try again.");
             }
        }

        public async Task CompleteAppointmentAsync(int id, string userId)
        {
            // Ensure fetch doctor details for validation
            var spec = new AppointmentWithDetailsSpec(id);
            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityWithAsync(spec);

            if (appointment == null) throw new KeyNotFoundException($"Appointment {id} not found");

            if (appointment.Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot complete a cancelled appointment.");

            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedAt = Now;

            _unitOfWork.Repository<Appointment>().Update(appointment);

            try
            {
                await _unitOfWork.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Status update failed due to concurrent modification.");
            }
        }
    }
}
