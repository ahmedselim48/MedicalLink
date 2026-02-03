using System.Security.Claims;
using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedLink.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    /// <summary>
    /// Manages user profile, dashboard, and settings.
    /// </summary>
    public class UserProfileController : ControllerBase
    {
        private readonly IProfileDashboardService _profileDashboardService;
        private readonly IUserLanguageService _userLanguageService;
        private readonly IProfileService _profileService;
        private readonly IImageService _imageService;
        private readonly IProfileAppointmentService _appointmentService;

        public UserProfileController(
            IProfileDashboardService profileDashboardService,
            IUserLanguageService userLanguageService,
            IProfileService profileService,
            IProfileAppointmentService appointmentService,
            IImageService imageService)
        {
            _profileDashboardService = profileDashboardService;
            _userLanguageService = userLanguageService;
            _profileService = profileService;
            _imageService = imageService;
            _appointmentService = appointmentService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue("uid") ?? throw new UnauthorizedAccessException("User not authenticated"); ;
        }



        /// <summary>
        /// Retrieves user dashboard statistics.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetMyDashboard()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dashboard = await _profileDashboardService.GetMyDashboardAsync(userId);
            return Ok(dashboard);
        }


        /// <summary>
        /// Retrieves current user profile details.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<EditProfileDto>> GetMyProfile()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _profileService.GetMyProfileAsync(
                userId);
            return Ok(profile);
        }

        /// <summary>
        /// Updates the user's profile information.
        /// </summary>
        /// <param name="dto">The updated profile data.</param>
        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _profileService.UpdateMyProfileAsync(userId, dto);
            return NoContent();
        }



        /// <summary>
        /// Updates the user's profile image.
        /// </summary>
        /// <param name="request">The image upload request.</param>
        [HttpPut("image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfileImage([FromForm] UploadImageRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (request.Image == null || request.Image.Length == 0)
                return BadRequest("Image is required");

            var imageUrl = await _imageService.UploadAsync(request.Image);

            await _profileService.UpdateProfileImageAsync(userId, imageUrl);
            return NoContent();
        }


        /// <summary>
        /// Removes the user's profile image.
        /// </summary>
        [HttpDelete("image")]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _profileService.RemoveProfileImageAsync(userId);
            return NoContent();
        }


        /// <summary>
        /// Retrieves past appointments.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Items per page.</param>
        [HttpGet("appointments/past")]
        public async Task<IActionResult> GetPast(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();

            var result = await _appointmentService
                .GetPastAsync(userId, page, pageSize);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves upcoming appointments.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Items per page.</param>
        [HttpGet("appointments/upcoming")]
        public async Task<IActionResult> GetUpcoming(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();

            var result = await _appointmentService
                .GetUpcomingAsync(userId, page, pageSize);

            return Ok(result);
        }


        /// <summary>
        /// Retrieves the user's preferred language.
        /// </summary>
        [HttpGet("language")]
        public async Task<IActionResult> GetPreferredLanguage()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var language = await _userLanguageService.GetPreferredLanguageAsync(userId);
            return Ok(language);
        }

        /// <summary>
        /// Updates the user's preferred language.
        /// </summary>
        /// <param name="dto">The language update details.</param>
        [HttpPut("language")]
        public async Task<IActionResult> UpdatePreferredLanguage([FromBody] UpdateLanguageDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _userLanguageService.UpdatePreferredLanguageAsync(userId, dto);
            return NoContent();
        }



    }
}

