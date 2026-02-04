using Medical_Team_B.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    /// <summary>
    /// Handles application-wide error responses.
    /// </summary>
    [Route("errors/{code}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        /// <summary>
        /// Redirects to a standardized error response based on the status code.
        /// </summary>
        /// <param name="code">The HTTP status code.</param>
        public IActionResult Error(int code)
        {
            return NotFound(new ApiResponse(code));
        }
    }
}
