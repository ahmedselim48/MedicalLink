using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.Content;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    [Route("api/languages")]
    [ApiController]
    /// <summary>
    /// Manages supported languages.
    /// </summary>
    public class LanguagesController : BaseApiController
    {
        private readonly ILanguageService _languageService;

        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        /// <summary>
        /// Retrieves all supported languages.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Language>>> GetAll()
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            return Ok(languages);
        }

        /// <summary>
        /// Retrieves a language by ID.
        /// </summary>
        /// <param name="id">The ID of the language.</param>
        [HttpGet("{id:int}", Name = "GetLanguageById")]
        public async Task<ActionResult<Language>> GetById(int id)
        {
            var language = await _languageService.GetLanguageByIdAsync(id);
            if (language == null) return NotFound();

            return Ok(language);
        }

        /// <summary>
        /// Adds a new language.
        /// </summary>
        /// <param name="language">The language details.</param>
        [HttpPost]
        public async Task<ActionResult<Language>> Create(Language language)
        {
            var createdLanguage = await _languageService.AddLanguageAsync(language);
            return CreatedAtRoute("GetLanguageById", new { id = createdLanguage.Id }, createdLanguage);
        }

        /// <summary>
        /// Updates a language.
        /// </summary>
        /// <param name="id">The ID of the language.</param>
        /// <param name="language">The updated language details.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Language language)
        {
            if (id != language.Id) return BadRequest("ID mismatch");

            await _languageService.UpdateLanguageAsync(language);
            return NoContent();
        }

        /// <summary>
        /// Deletes a language.
        /// </summary>
        /// <param name="id">The ID of the language to delete.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _languageService.DeleteLanguageAsync(id);
            return NoContent();
        }
    }
}
