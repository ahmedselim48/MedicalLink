using MedLink.Application.Interfaces.Services;
using MedLink.Application.Mapping;
using MedLink.Application.Services;
using MedLink.Domain.Interfaces.Repositories;
using MedLink.Infrastructure.Persistence.Context;
using MedLink.Infrastructure.Persistence.Repositories;
using MedLink.Application.Interfaces.Repositories;

namespace Medical_Team_B.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Core Services
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<ISpecializationService, SpecializationService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IProfileDashboardService, ProfileDashboardService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IUserLanguageService, UserLanguageService>();
            services.AddScoped<IProfileAppointmentService, ProfileAppointmentService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IFAQ, FAQService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<IAboutService, AboutService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IDoctorAvailabilityService, DoctorAvailabilityService>();
            services.AddScoped<IStripeWebhookService, StripeWebhookService>();
            services.AddScoped<IStripeService, StripeService>();

            // Repositories
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                });

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            return services;
        }
    }
}
