using MedLink.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedLink.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AppointmentId)
            .IsRequired();



        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .HasMaxLength(10);

        builder.Property(p => p.StripePaymentIntentId)
            .HasMaxLength(255);



        builder.Property(p => p.Status)
            .IsRequired();

        builder.HasOne(p => p.Appointment)
        .WithOne(a => a.Payment)
        .HasForeignKey<Payment>(p => p.AppointmentId)
        .OnDelete(DeleteBehavior.Cascade);

    }
}
