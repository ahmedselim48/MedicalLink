using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications.Appointments;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Enums;

namespace MedLink.Application.Services
{
    public class ProfileAppointmentService : IProfileAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProfileAppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public async Task CancelAsync(string userId, int appointmentId, string? reason)
        //{
        //    var appointment = await _unitOfWork
        //        .Repository<Appointment>()
        //        .GetByIdAsync(appointmentId);

        //    if (appointment == null)
        //        throw new Exception("Appointment not found");

        //    if (appointment.UserId != userId)
        //        throw new UnauthorizedAccessException();

        //    if (appointment.Status == AppointmentStatus.Cancelled)
        //        return;

        //    var isPast =
        //        appointment.Schedule.Date < DateTime.UtcNow.Date ||
        //        (
        //            appointment.Schedule.Date == DateTime.UtcNow.Date &&
        //            appointment.Schedule.StartTime < DateTime.UtcNow.TimeOfDay
        //        );

        //    if (isPast)
        //        throw new Exception("Cannot cancel past appointment");

        //    appointment.Status = AppointmentStatus.Cancelled;
        //    appointment.CancelledReason = reason;

        //    var availability = await _unitOfWork
        //        .Repository<DoctorAvailability>()
        //        .GetByIdAsync(appointment.ScheduleId);

        //    if (availability != null)
        //    {
        //        availability.IsBooked = false;
        //        _unitOfWork.Repository<DoctorAvailability>().Update(availability);
        //    }

        //    _unitOfWork.Repository<Appointment>().Update(appointment);
        //    await _unitOfWork.Complete();
        //}


        public async Task<PagedResult<AppointmentListItemDto>> GetPastAsync(string userId, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var dataSpec = new PastAppointmentsByUserSpec(userId, page, pageSize);
            var countSpec = new PastAppointmentsByUserSpec(userId);

            var totalCount = await _unitOfWork
                .Repository<Appointment>()
                .GetCountAsync(countSpec);

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllWithSpecAsync(dataSpec);

            var items = appointments.Select(a => new AppointmentListItemDto
            {
                AppointmentId = a.Id,
                DoctorName = a.Doctor.Name,
                DoctorImageUrl = a.Doctor.ImageUrl ?? "",
                Specialization = a.Doctor.Specialization.Name,
                Date = a.Schedule.Date,
                StartTime = a.Schedule.StartTime,
                Status = a.Status,
                CanCancel = false
            }).ToList();

            return new PagedResult<AppointmentListItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResult<AppointmentListItemDto>> GetUpcomingAsync(string userId, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var dataSpec = new UpcomingAppointmentsByUserSpec(userId, page, pageSize);
            var countSpec = new UpcomingAppointmentsByUserSpec(userId);

            var totalCount = await _unitOfWork
                .Repository<Appointment>()
                .GetCountAsync(countSpec);

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllWithSpecAsync(dataSpec);

            var now = DateTime.UtcNow;

            var items = appointments.Select(a =>
            {
                var appointmentDateTime =
                    a.Schedule.Date.Date + a.Schedule.StartTime;

                var canCancel =
                    (a.Status == AppointmentStatus.Pending ||
                     a.Status == AppointmentStatus.Confirmed)
                    && appointmentDateTime > now;

                return new AppointmentListItemDto
                {
                    AppointmentId = a.Id,
                    DoctorName = a.Doctor.Name,
                    DoctorImageUrl = a.Doctor.ImageUrl ?? "",
                    Specialization = a.Doctor.Specialization.Name,
                    Date = a.Schedule.Date,
                    StartTime = a.Schedule.StartTime,
                    Status = a.Status,
                    CanCancel = canCancel
                };
            }).ToList();

            return new PagedResult<AppointmentListItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}