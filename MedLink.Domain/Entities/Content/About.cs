using MedLink.Domain.Common;

namespace MedLink.Domain.Entities.Content;

public class About : BaseEntity
{
    public string TermsOfService { get; set; }
    public string PrivacyPolicy { get; set; }
    public string ReleaseNotes { get; set; }

    public string ContactInformation { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
