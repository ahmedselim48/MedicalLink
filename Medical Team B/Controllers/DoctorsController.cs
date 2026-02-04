using MedLink.Application.DTOs.Doctors;
using MedLink.Application.DTOs.Doctors;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Services;
using MedLink.Application.Specifications;
using MedLink.Domain.Entities.Medical;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    /// <summary>
    /// Manages doctor-related operations.
    /// </summary>
    /// 
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : BaseApiController
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// Searches for doctors based on criteria.
        /// </summary>
        /// <param name="searchParams">Filters for doctor search (e.g., keyword, specialty, location, date).</param>
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<DoctorSearchResultDto>>> Search(
            [FromQuery] DoctorSearchParams searchParams)
        {
            var doctors = await _doctorService.SearchDoctorsAsync(searchParams);
            return Ok(doctors);
        }
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Doctor>>> GetAll()
        {
            var spec = new BaseSpecification<Doctor>(s => true);
            var result = await _doctorService.GetAllDoctorsAsync(spec);
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<Doctor>> Create(Doctor doctor)
        {
           
            var createdDoctor = await _doctorService.AddDoctorAsync(doctor);

            return CreatedAtAction(nameof(GetById), new { id = createdDoctor.Id }, createdDoctor);
        }

      
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Doctor doctor)
        {
            if (id != doctor.Id)
                return BadRequest("ID mismatch");

            await _doctorService.UpdateDoctorAsync(doctor);
            return NoContent();
        }

       
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _doctorService.DeleteDoctorAsync(id);
            return NoContent();
        }

        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Doctor>> GetById(int id)
        {
           
            return Ok();
        }

    }
}
