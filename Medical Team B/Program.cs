using Mapster;
using Medical_Team_B.Extensions;
using Medical_Team_B.Middlewares;
using MedicalSystem.API.Hubs;
using MedLink.Domain.Identity;
using MedLink.Infrastructure.Persistence.Context;
using MedLink.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSwaggerGenJwtAuth();
builder.Services.AddCors(option =>
        option.AddPolicy("MyPolicy", builder =>

        builder.AllowAnyOrigin()
        .AllowAnyHeader().AllowAnyMethod()

        )

        );
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // للتطوير فقط
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null; // للحفاظ على الحالة
});
builder.Services.AddMapster();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Swagger with JWT support
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Apply database migrations
await app.ApplyDatabaseMigrationsAsync();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.MapHub<ChatHub>("/chatHub");
app.UseHttpsRedirection();
app.UseStaticFiles()
;
app.UseCors("MyPolicy");
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
