using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.Content;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Manages 'About Us' content.
    /// </summary>
    public class AboutController : ControllerBase
    {
        private readonly IAboutService _aboutService;

        public AboutController(IAboutService aboutService)
        {
            _aboutService = aboutService;
        }


        /// <summary>
        /// Retrieves About content by ID.
        /// </summary>
        /// <param name="id">The ID of the content.</param>
        [HttpGet("{id:int}", Name = "GetAboutById")]
        public async Task<ActionResult<About>> GetById(int id)
        {
            var about = await _aboutService.GetAboutByIdAsync(id);
            if (about == null) return NotFound();

            return Ok(about);
        }


        /// <summary>
        /// Creates new About content.
        /// </summary>
        /// <param name="about">The content to create.</param>
        [HttpPost]
        public async Task<ActionResult<About>> Create(About about)
        {

            about.LastUpdated = DateTime.UtcNow;

            var createdAbout = await _aboutService.AddAboutAsync(about);

            return CreatedAtRoute("GetAboutById", new { id = createdAbout.Id }, createdAbout);
        }


        /// <summary>
        /// Updates About content.
        /// </summary>
        /// <param name="id">The ID of the content.</param>
        /// <param name="about">The updated content.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, About about)
        {
            if (id != about.Id) return BadRequest("ID mismatch");

            about.LastUpdated = DateTime.UtcNow;
            await _aboutService.UpdateAboutAsync(about);
            return NoContent();
        }


        /// <summary>
        /// Deletes About content.
        /// </summary>
        /// <param name="id">The ID of the content to delete.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _aboutService.DeleteAboutAsync(id);
            return NoContent();
        }
    }
}
