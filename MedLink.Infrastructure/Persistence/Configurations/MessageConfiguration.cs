using MedLink.Domain.Entities.Chat;
using MedLink.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedLink.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ChatRoomId)
            .IsRequired();

       

        builder.HasOne<ApplicationUser>()
            .WithMany()
           
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(m => m.ChatRoom)
            .WithMany(cr => cr.Messages)
            .HasForeignKey(m => m.ChatRoomId);
    }
}
