using MedLink.Domain.Entities.Medical;
using Microsoft.AspNetCore.Identity;

namespace MedLink.Domain.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public Doctor? Doctor { get; set; }
}
