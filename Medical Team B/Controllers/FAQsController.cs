using AutoMapper;
using FluentValidation;
using MedLink.Application.DTOs.Settings;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Services;
using MedLink.Application.Specifications;
using MedLink.Domain.Entities.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Medical_Team_B.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Manages Frequently Asked Questions.
    /// </summary>
    public class FAQsController : ControllerBase
    {
        private readonly IFAQ _fAQ ;
        private readonly IProfileService _userProfileService; 
        public FAQsController(IFAQ fAQ, IProfileService profileService)
        {
           _fAQ = fAQ;
            _userProfileService = profileService;
        }
        /// <summary>
        /// Retrieves all FAQs.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FAQ>>> GetAll()
        {
            var spec = new BaseSpecification<FAQ>(s => true);
            var result = await _fAQ.GetAllQuestionsAsync(spec);
            return Ok(result);
        }




        /// <summary>
        /// Retrieves an FAQ by ID.
        /// </summary>
        /// <param name="id">The ID of the FAQ.</param>
        [HttpGet("{id:int}", Name = "GetQuestionByIdAsync")]
        public async Task<ActionResult<FAQ>> GetById(Guid id)
        {
            var q = await _fAQ.GetQuestionByIdAsync(id);
            if (q == null)
                return NotFound();

            return Ok(q);
        }

        [HttpPost]
        public async Task<ActionResult<FAQ>> Create([FromBody] FAQ q, [FromServices] IValidator<FAQ> validator)
        {
           
            var validationResult = await validator.ValidateAsync(q);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            try
            {
                var created = await _fAQ.CreateQuestionAsync(q);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
               
                return BadRequest(new { message = ex.Message });
            }
        }
        //   [Authorize(Roles ="User")]
        [HttpPut("{id}/answer")]
        public async Task<IActionResult> SubmitAnswer(int id, [FromBody] AnswerRequestDto request)
        {
            // 1. نجيب الـ UserId من الـ Token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. نجيب الـ UserProfile المربوط بالـ UserId ده
            var profile = await _userProfileService.GetProfileByUserIdAsync(userId);

            if (profile == null) return BadRequest("User profile not found.");

            // 3. هنا بنستخدم profile.Id (لأن profileId مش متعرف لوحده)
            await _fAQ.SubmitAnswerAsync(id, request.Answer, profile.Id);

            return Ok(new { message = "Answer submitted successfully" });
        }

    }
}
