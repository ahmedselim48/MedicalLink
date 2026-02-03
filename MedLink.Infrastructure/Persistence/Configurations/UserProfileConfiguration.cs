using MedLink.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedLink.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(up => up.Id);

        builder.HasIndex(x => x.UserId)
               .IsUnique();

        builder.Property(up => up.UserId)
               .IsRequired();

        builder.Property(up => up.FullName)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(up => up.ImageUrl)
               .HasMaxLength(512);

        builder.Property(up => up.MedicalHistory)
               .HasMaxLength(2000);
    }
}
