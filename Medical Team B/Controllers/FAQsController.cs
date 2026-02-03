using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications;
using MedLink.Domain.Entities.Content;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Manages Frequently Asked Questions.
    /// </summary>
    public class FAQsController : ControllerBase
    {
        private readonly IFAQ _fAQ;

        public FAQsController(IFAQ fAQ)
        {
            _fAQ = fAQ;
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


        /// <summary>
        /// Creates a new FAQ.
        /// </summary>
        /// <param name="q">The FAQ content.</param>
        [HttpPost]
        public async Task<ActionResult<FAQ>> Create([FromBody] FAQ q)
        {
            var created = await _fAQ.CreateQuestionAsync(q);

            return CreatedAtAction(nameof(GetById), new { id = q.Id }, q);
        }



        /// <summary>
        /// Updates an existing FAQ.
        /// </summary>
        /// <param name="id">The ID of the FAQ.</param>
        /// <param name="q">The updated FAQ content.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FAQ q)
        {
            if (id != q.Id)
                return BadRequest("ID mismatch");

            await _fAQ.UpdateQuestionAsync(q);
            return NoContent();
        }

        // DELETE
        //[HttpDelete("{id:Guid}")]
        //public async Task<IActionResult> Delete(Guid id) // تغيير Guid لـ int
        //{
        //    await _specializationService.DeleteSpecializationAsync(id);
        //    return NoContent();
        //}
    }
}
