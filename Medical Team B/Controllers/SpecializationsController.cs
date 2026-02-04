using MedLink.Application.DTOs.Medical;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications;
using MedLink.Domain.Entities.Medical;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Manages medical specializations.
    /// </summary>
    public class SpecializationsController : ControllerBase
    {
        private readonly ISpecializationService _specializationService;

        public SpecializationsController(ISpecializationService specializationService)
        {
            _specializationService = specializationService;
        }


        /// <summary>
        /// Retrieves featured specializations (Dto version).
        /// </summary>
        /// <param name="count">Optional number of specializations to return.</param>
        [HttpGet("featured")]
        public async Task<ActionResult<IReadOnlyList<SpecializationDto>>> GetFeatured([FromQuery] int? count)
        {
            var specialties = await _specializationService.GetAllSpecializationsAsync(count);
            return Ok(specialties);
        }

        /// <summary>
        /// Retrieves all specializations.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Specialization>>> GetAll()
        {
            var spec = new BaseSpecification<Specialization>(s => true);
            var result = await _specializationService.GetAllSpecializationsAsync(spec);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all specializations including associated doctors.
        /// </summary>
        [HttpGet("with-doctors")]
        public async Task<ActionResult<IReadOnlyList<Specialization>>> GetAllWithDoctors()
        {

            var spec = new BaseSpecification<Specialization>(s => true);


            spec.Includes.Add(s => s.Doctors);

            var result = await _specializationService.GetAllSpecializationsAsync(spec);
            return Ok(result);
        }


        /// <summary>
        /// Retrieves a specialization by ID.
        /// </summary>
        /// <param name="id">The ID of the specialization.</param>
        [HttpGet("{id:int}", Name = "GetSpecializationById")]
        public async Task<ActionResult<Specialization>> GetById(int id) // تغيير Guid لـ int
        {
            var specialization = await _specializationService.GetSpecializationByIdAsync(id);
            if (specialization == null)
                return NotFound();

            return Ok(specialization);
        }


        /// <summary>
        /// Creates a new specialization.
        /// </summary>
        /// <param name="specialization">The specialization details.</param>
        [HttpPost]
        public async Task<ActionResult<Specialization>> Create([FromBody] Specialization specialization)
        {
            var created = await _specializationService.CreateSpecializationAsync(specialization);

            return CreatedAtAction(nameof(GetById), new { id = specialization.Id }, specialization);
        }



        /// <summary>
        /// Updates an existing specialization.
        /// </summary>
        /// <param name="id">The ID of the specialization.</param>
        /// <param name="specialization">The updated specialization details.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Specialization specialization)
        {
            if (id != specialization.Id)
                return BadRequest("ID mismatch");

            await _specializationService.UpdateSpecializationAsync(specialization);
            return NoContent();
        }


        /// <summary>
        /// Deletes a specialization.
        /// </summary>
        /// <param name="id">The ID of the specialization to delete.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _specializationService.DeleteSpecializationAsync(id);
            return NoContent();
        }
    }
}
