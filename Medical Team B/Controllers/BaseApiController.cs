using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Base API controller providing common functionality for all controllers.
    /// </summary>
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Gets the current authenticated user's ID.
        /// </summary>
        protected string UserId => User.FindFirstValue("uid")!;
    }
}
