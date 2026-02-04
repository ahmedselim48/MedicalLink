namespace MedLink.Application.DTOs.UserProfile
{
    public class ProfileHeaderDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime MemberSince { get; set; }
    }
}
