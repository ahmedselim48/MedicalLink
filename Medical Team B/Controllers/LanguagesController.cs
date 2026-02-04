using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class LanguagesController : ControllerBase
    {
        private readonly ILanguageService _languageService;
        public LanguagesController(ILanguageService languageService)
        {
            _languageService = languageService;
        }
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Language>>> GetAll()
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            return Ok(languages);
        }

       
        [HttpGet("{id:int}", Name = "GetLanguageById")]
        public async Task<ActionResult<Language>> GetById(int id)
        {
            var language = await _languageService.GetLanguageByIdAsync(id);
            if (language == null) return NotFound();

            return Ok(language);
        }

       
        [HttpPost]
        public async Task<ActionResult<Language>> Create(Language language)
        {
            var createdLanguage = await _languageService.AddLanguageAsync(language);

            
            return CreatedAtRoute("GetLanguageById", new { id = createdLanguage.Id }, createdLanguage);
        }

       
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Language language)
        {
            if (id != language.Id) return BadRequest("ID mismatch");

            await _languageService.UpdateLanguageAsync(language);
            return NoContent();
        }

       
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _languageService.DeleteLanguageAsync(id);
            return NoContent();
        }
    }
}
