using System.Security.Claims;

namespace Medical_Team_B.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            // في ASP.NET Core Identity، الـ UserId بيكون string
            return principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   principal.FindFirstValue("sub") ??
                   string.Empty;
        }
    }
}
