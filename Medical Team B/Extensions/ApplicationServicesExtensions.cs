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
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<ISpecializationService,SpecializationService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IFAQ,FAQService>();
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
            // ›Ì „·› Program.cs
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // «·”ÿ— œÂ ÂÊ «··Ì ÂÌŒ·Ì ⁄·«ﬁ… «·‹ Specialization Ê«·‹ Doctors  ŸÂ— ﬂ«„·… ›Ì «·‹ JSON
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

                    // «Œ Ì«— ≈÷«›Ì: ·Ê ⁄«Ì“… «·‹ JSON Ì—Ã⁄ »‰›” √”„«¡ «·‹ Properties “Ì „« ÂÌ ›Ì «·‹ C#
                    // options.JsonSerializerOptions.PropertyNamingPolicy = null; 
                });

            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
