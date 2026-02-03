using System.Reflection;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Chat;
using MedLink.Domain.Entities.Content;
using MedLink.Domain.Entities.Medical;
using MedLink.Domain.Entities.Payments;
using MedLink.Domain.Entities.User;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace MedLink.Infrastructure.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Specialization> Specializations { get; set; }

    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
    public DbSet<Review> Reviews { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<About> Abouts { get; set; }
    public DbSet<Language> Languages { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Favorite> Favorites { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Doctor>().HasQueryFilter(d => !d.IsDeleted);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
