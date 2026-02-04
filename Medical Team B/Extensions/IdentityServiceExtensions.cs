using System.Text;
using MedLink.Application.Common.Email;
using MedLink.Application.Common.JWT;
using MedLink.Application.Common.Sms;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Services;
using MedLink.Domain.Identity;
using MedLink.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Medical_Team_B.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<Jwt>(configuration.GetSection("Jwt"));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
         .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();



            var emailConfig = configuration.GetSection("EmailConfiguration")
        .Get<EmailConfigurations>();

            services.AddSingleton(emailConfig);

            services.AddScoped(typeof(IEmailService), typeof(EmailService));

            services.Configure<TwilioSettings>(configuration.GetSection("Twilio"));
            services.AddTransient<ISmsService, SmsService>();



            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(O =>
                {
                    O.RequireHttpsMetadata = false;
                    O.SaveToken = false;
                    O.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                    };
                })
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                });



            return services;
        }
    }
}
