using Medical_Team_B.Errors;
using MedLink.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    /// <summary>
    /// Controller used for testing various error responses and edge cases.
    /// </summary>
    public class BuggyController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public BuggyController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a 404 NotFound error.
        /// </summary>
        [HttpGet("notfound")]
        public ActionResult GetNotFound()
        {
            return NotFound(new ApiResponse(404));
        }

        //[HttpGet("servererror")]
        //public ActionResult GetServerError()
        //{
        //    var users = _context.Users.Find(100);
        //    var usersToReturn = users.ToString();
        //    return Ok(usersToReturn);
        //}

        /// <summary>
        /// Returns a 400 BadRequest error.
        /// </summary>
        [HttpGet("badrequest")]
        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiResponse(400));
        }

        /// <summary>
        /// Returns a 400 BadRequest error with a parameter.
        /// </summary>
        /// <param name="id">Test ID.</param>
        [HttpGet("badrequest/{id}")]
        public ActionResult GetBadRequest(int id)
        {
            return Ok();
        }



    }
}
