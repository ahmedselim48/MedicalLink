using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities.User
{
    public class UserProfile : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }
        public string? MedicalHistory { get; set; }

        public string PreferredLanguage { get; set; } = "en";

    }
}
