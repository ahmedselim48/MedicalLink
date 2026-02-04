using MedLink.Domain.Common;
using MedLink.Domain.Entities.User;

namespace MedLink.Domain.Entities.Content;

public class FAQ : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int? AnsweredByProfileId { get; set; }
    public UserProfile? AnsweredByProfile { get; set; }
}
