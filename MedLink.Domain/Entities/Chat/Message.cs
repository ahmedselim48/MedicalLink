using MedLink.Domain.Common;
using MedLink.Domain.Identity;

namespace MedLink.Domain.Entities.Chat;

public class Message : BaseEntity
{
    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = null!;

    public string? SenderId { get; set; } = string.Empty;
    public ApplicationUser? Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    // Legacy seed support
    public string SenderUserId { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public bool IsEdited { get; set;}

    }
