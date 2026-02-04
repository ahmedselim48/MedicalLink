using MedLink.Application.DTOs.Doctors;
using MedLink.Application.DTOs.User;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    /// <summary>
    /// Manages user favorite doctors.
    /// </summary>
    [Authorize]
    public class FavoritesController : BaseApiController
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Gets the current user's favorite doctors.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DoctorSearchResultDto>>> GetFavorites()
        {
            var favorites = await _favoriteService.GetUserFavoritesAsync(UserId);
            return Ok(favorites);
        }

        /// <summary>
        /// Adds a doctor to favorites.
        /// </summary>
        /// <param name="favoriteDto">Contains the DoctorId to add.</param>
        [HttpPost]
        public async Task<ActionResult> AddFavorite([FromBody] FavoriteDto favoriteDto)
        {
            await _favoriteService.AddFavoriteAsync(UserId, favoriteDto.DoctorId);
            return StatusCode(StatusCodes.Status201Created, new { message = "Added to favorites successfully" });
        }

        /// <summary>
        /// Removes a doctor from favorites.
        /// </summary>
        /// <param name="doctorId">The ID of the doctor to remove from favorites.</param>
        [HttpDelete("{doctorId}")]
        public async Task<ActionResult> RemoveFavorite(int doctorId)
        {
            await _favoriteService.RemoveFavoriteAsync(UserId, doctorId);
            return Ok(new { message = "Removed from favorites successfully" });
        }
    }
}
