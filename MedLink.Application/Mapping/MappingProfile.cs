using AutoMapper;
using MedLink.Application.DTOs.Appointments;
using MedLink.Application.DTOs.Doctors;
using MedLink.Application.DTOs.Medical;
using MedLink.Application.DTOs.Payments;
using MedLink.Application.DTOs.Identity;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Medical;
using MedLink.Domain.Entities.Payments;
using MedLink.Domain.Identity;

namespace MedLink.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Identity & Auth Mappings
            CreateMap<RegisterModel, ApplicationUser>()
             .ForMember(d => d.UserName, o => o.MapFrom<EmailToUsernameResolver>())
             .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Contains("@") ? s.Email : null));

            // Doctor & Search Mappings
            CreateMap<Doctor, DoctorSearchResultDto>()
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialization.Name))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0))
                .ForMember(dest => dest.AvailableToday, opt => opt.MapFrom(src => src.Availabilities.Any(a => a.Date.Date == DateTime.Today && !a.IsBooked)));

            CreateMap<Doctor, DoctorDto>()
                .ForMember(dest => dest.SpecializationName,
                    opt => opt.MapFrom(src => src.Specialization != null ? src.Specialization.Name : string.Empty))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.Gender.ToString()));

            // Specialization Mappings
            CreateMap<Specialization, SpecializationDto>();

            // Appointment Mappings
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Name : string.Empty))
                .ForMember(dest => dest.DoctorSpecialization,
                    opt => opt.MapFrom(src => (src.Doctor != null && src.Doctor.Specialization != null) ? src.Doctor.Specialization.Name : string.Empty))
                .ForMember(dest => dest.Date,
                    opt => opt.MapFrom(src => src.Schedule.Date))
                .ForMember(dest => dest.StartTime,
                    opt => opt.MapFrom(src => src.Schedule.StartTime))
                .ForMember(dest => dest.EndTime,
                    opt => opt.MapFrom(src => src.Schedule.EndTime))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentId,
                    opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Id : (int?)null))
                .ForMember(dest => dest.PaymentStatus,
                    opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Status.ToString() : null));

            // DoctorAvailability Mappings
            CreateMap<DoctorAvailability, DoctorAvailabilityDto>()
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Name : string.Empty));

            // Payment Mappings
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Method,
                    opt => opt.MapFrom(src => src.Method.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CheckoutSessionId,
                    opt => opt.MapFrom(src => src.CheckoutSessionId))
                .ForMember(dest => dest.CheckoutUrl,
                    opt => opt.MapFrom(src => src.CheckoutUrl));
        }
    }
}
