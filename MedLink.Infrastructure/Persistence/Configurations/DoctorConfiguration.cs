using MedLink.Domain.Entities.Medical;
using MedLink.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedLink.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.SpecialtyId).IsRequired();

		builder.HasOne(d => d.User)
			.WithOne(u => u.Doctor)
			.HasForeignKey<Doctor>(d => d.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasIndex(d => d.UserId)
			   .IsUnique()
			   .HasFilter("[UserId] IS NOT NULL");

		builder.HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.Bio).HasMaxLength(2000);
        builder.Property(d => d.ImageUrl).HasMaxLength(512);
        builder.Property(d => d.Price).HasPrecision(18, 2);
        builder.Property(d => d.ClinicDetails).HasMaxLength(1000);

        builder.HasOne(d => d.Specialization)
            .WithMany(s => s.Doctors)
            .HasForeignKey(d => d.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.Location)
            .HasColumnType("geography");

        builder.Property(d => d.Address);

        builder.Property(d => d.City)
            .IsRequired();

        builder.Property(d => d.Gender)
            .IsRequired();

        builder.HasMany(d => d.Availabilities)
            .WithOne(av => av.Doctor)
            .HasForeignKey(av => av.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);


        //   builder.Ignore(d => d.Location);



        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId);

        builder.HasMany(d => d.Reviews)
            .WithOne(r => r.Doctor)
            .HasForeignKey(r => r.DoctorId);

        builder.HasIndex(d => new { d.City, d.SpecialtyId });
    }
}
