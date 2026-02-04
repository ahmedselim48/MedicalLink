using FluentValidation;
using FluentValidation.AspNetCore;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Mapping;
using MedLink.Application.Services;
using MedLink.Application.Validators;

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
            
           services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<DoctorValidator>();
            services.AddValidatorsFromAssemblyContaining<SpecializationValidator>();
            // Register the Profile Service
            services.AddScoped<IProfileService, ProfileService>();
            //   services.AddValidatorsFromAssemblyContaining<FAQValidator>();
            services.AddControllers()
    .AddJsonOptions(options => {
       options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
    });
            // Ýí ãáÝ Program.cs
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // ÇáÓØÑ Ïå åæ Çááí åíÎáí ÚáÇÞÉ ÇáÜ Specialization æÇáÜ Doctors ÊÙåÑ ßÇãáÉ Ýí ÇáÜ JSON
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

                    // ÇÎÊíÇÑ ÅÖÇÝí: áæ ÚÇíÒÉ ÇáÜ JSON íÑÌÚ ÈäÝÓ ÃÓãÇÁ ÇáÜ Properties Òí ãÇ åí Ýí ÇáÜ C#
                    // options.JsonSerializerOptions.PropertyNamingPolicy = null; 
                });

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            return services;
        }
    }
}
