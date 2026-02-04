using AutoMapper;
using MedLink.Application.DTOs.Appointments;
using MedLink.Application.DTOs.Medical;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications.Appointments;
using MedLink.Application.Specifications.Medical;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Medical;
using Microsoft.Extensions.Logging;

namespace MedLink.Application.Services
{
    public class DoctorAvailabilityService : IDoctorAvailabilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorAvailabilityService> _logger;

        public DoctorAvailabilityService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            ILogger<DoctorAvailabilityService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DoctorAvailabilityDto> AddSingleSlotAsync(AddSlotRequest request, string? userId = null)
        {
            int doctorId = await ResolveDoctorIdAsync(request.DoctorId, userId);
            
            await ValidateDoctorExists(doctorId);

            var startTime = TimeSpan.Parse(request.StartTime);
            var endTime = TimeSpan.Parse(request.EndTime);

            if (startTime >= endTime)
                throw new InvalidOperationException("Start time must be before end time.");

            var slot = new DoctorAvailability
            {
                DoctorId = doctorId,
                Date = request.Date.Date,
                StartTime = startTime,
                EndTime = endTime,
                IsBooked = false,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Repository<DoctorAvailability>().AddAsync(slot);
            await _unitOfWork.Complete();

            return _mapper.Map<DoctorAvailabilityDto>(slot);
        }

        public async Task<List<DoctorAvailabilityDto>> AddDayScheduleAsync(AddDayScheduleRequest request, string? userId = null)
        {
            int doctorId = await ResolveDoctorIdAsync(request.DoctorId, userId);

            await ValidateDoctorExists(doctorId);

            var start = TimeSpan.Parse(request.StartTime);
            var end = TimeSpan.Parse(request.EndTime);
            var duration = request.SlotDurationMinutes ?? 15;

            var slots = new List<DoctorAvailability>();
            var cursor = start;

            while (cursor.Add(TimeSpan.FromMinutes(duration)) <= end)
            {
                var slotEnd = cursor.Add(TimeSpan.FromMinutes(duration));

                slots.Add(new DoctorAvailability
                {
                    DoctorId = doctorId,
                    Date = request.Date.Date,
                    StartTime = cursor,
                    EndTime = slotEnd,
                    IsBooked = false,
                    UpdatedAt = DateTime.UtcNow
                });

                cursor = slotEnd;
            }

            foreach (var slot in slots)
            {
                await _unitOfWork.Repository<DoctorAvailability>().AddAsync(slot);
            }

            await _unitOfWork.Complete();
            return _mapper.Map<List<DoctorAvailabilityDto>>(slots);
        }

        public async Task<List<DoctorAvailabilityDto>> AddWeekScheduleAsync(AddWeekScheduleRequest request, string? userId = null)
        {
             int doctorId = await ResolveDoctorIdAsync(request.DoctorId, userId);

             await ValidateDoctorExists(doctorId);

             var createdSlots = new List<DoctorAvailability>();
             var currentDate = request.StartDate.Date;
             var start = TimeSpan.Parse(request.StartTime);
             var end = TimeSpan.Parse(request.EndTime);
             var duration = request.SlotDurationMinutes;

             // Generate for 7 days
             for (int i = 0; i < 7; i++)
             {
                 if (request.WorkDays.Contains(currentDate.DayOfWeek))
                 {
                     // Add slots for this day
                    var cursor = start;
                    while (cursor.Add(TimeSpan.FromMinutes(duration)) <= end)
                    {
                        var slotEnd = cursor.Add(TimeSpan.FromMinutes(duration));
                        var slot = new DoctorAvailability
                        {
                            DoctorId = doctorId,
                            Date = currentDate,
                            StartTime = cursor,
                            EndTime = slotEnd,
                            IsBooked = false,
                            UpdatedAt = DateTime.UtcNow
                        };
                        createdSlots.Add(slot);
                        await _unitOfWork.Repository<DoctorAvailability>().AddAsync(slot);

                        cursor = slotEnd;
                    }
                 }
                 currentDate = currentDate.AddDays(1);
             }

             await _unitOfWork.Complete();
             return _mapper.Map<List<DoctorAvailabilityDto>>(createdSlots);
        }

        public async Task DeleteSlotAsync(int slotId)
        {
            var slot = await _unitOfWork.Repository<DoctorAvailability>().GetByIdAsync(slotId);
            if (slot == null) throw new KeyNotFoundException("Slot not found");

            if (slot.IsBooked)
                throw new InvalidOperationException("Cannot delete a booked slot. Cancel the appointment first.");

            _unitOfWork.Repository<DoctorAvailability>().Delete(slot);
            await _unitOfWork.Complete();
        }

        public async Task<List<DoctorAvailabilityDto>> GetAllDoctorSlotsAsync(int doctorId)
        {
            var spec = new DoctorAvailabilitySpec(doctorId); 
            var slots = await _unitOfWork.Repository<DoctorAvailability>().GetAllWithSpecAsync(spec);
            return _mapper.Map<List<DoctorAvailabilityDto>>(slots);
        }

        public async Task<List<DoctorAvailabilityDto>> GetAvailableSlotsAsync(int doctorId, DateTime? date = null)
        {
             var spec = new DoctorAvailabilitySpec(doctorId, date, isAvailable: true);
             var slots = await _unitOfWork.Repository<DoctorAvailability>().GetAllWithSpecAsync(spec);
             return _mapper.Map<List<DoctorAvailabilityDto>>(slots);
        }

        private async Task ValidateDoctorExists(int doctorId)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
            if (doctor == null) throw new KeyNotFoundException($"Doctor {doctorId} not found");
        }

        public async Task<int> GetDoctorIdByUserIdAsync(string userId)
        {
            // Use FindAsync with predicate directly to bypass Spec logic for debugging
            var doctor = await _unitOfWork.Repository<Doctor>().FindAsync(d => d.UserId == userId);
            
            if (doctor == null) 
                throw new KeyNotFoundException($"No doctor profile found linked to UserID: {userId}. Please ensure Doctor record exists and UserId column matches.");
                
            return doctor.Id;
        }

        private async Task<int> ResolveDoctorIdAsync(int? requestDoctorId, string? userId)
        {
            // If userId is present, it takes precedence (Doctor scenario)
            if (!string.IsNullOrEmpty(userId))
            {
                var id = await GetDoctorIdByUserIdAsync(userId);
                // Optionally verify if requestDoctorId was passed and matches
                if (requestDoctorId.HasValue && requestDoctorId.Value != id && requestDoctorId.Value != 0)
                {
                    throw new UnauthorizedAccessException("Cannot manage slots for another doctor.");
                }
                return id;
            }

            // Fallback for Admin or manual ID
            if (requestDoctorId.HasValue && requestDoctorId.Value != 0)
            {
               return requestDoctorId.Value;
            }

            throw new ArgumentException("DoctorId is required.");
        }
    }
}
