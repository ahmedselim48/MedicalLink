using Mapster;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Mapping;
using MedLink.Application.Services;
using MedLink.Infrastructure.Persistence.Repositories;

namespace Medical_Team_B.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<ISpecializationService, SpecializationService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IPresenceService, PresenceService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IChatRoomService, ChatRoomService>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            

            TypeAdapterConfig.GlobalSettings.Scan(typeof(ChatMappingConfig).Assembly);

            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
