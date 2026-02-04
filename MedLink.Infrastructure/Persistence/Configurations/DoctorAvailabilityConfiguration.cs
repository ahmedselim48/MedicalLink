using MedLink.Domain.Entities.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedLink.Infrastructure.Persistence.Configurations
{
    public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
    {
        public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
        {
            builder.HasKey(da => da.Id);

            builder.Property(da => da.DoctorId)
                .IsRequired();

            builder.Property(da => da.Date)
                .IsRequired();

            builder.Property(da => da.StartTime)
                .IsRequired();

            builder.Property(da => da.EndTime)
                .IsRequired();

            builder.Property(da => da.IsBooked)
                .IsConcurrencyToken()
                .HasDefaultValue(false);

            builder.HasOne(da => da.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(da => da.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(da => da.Appointment)
                .WithOne(a => a.Schedule)
                .HasForeignKey<Appointment>(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(da => new { da.DoctorId, da.Date });
        }
    }
}