using AutoMapper;
using MedLink.Application.DTOs.Doctors;
using MedLink.Application.DTOs.Identity;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications;
using MedLink.Domain.Entities.Medical;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Team_B.Controllers
{
    /// <summary>
    /// Manages doctor-related operations.
    /// </summary>
    public class DoctorsController : BaseApiController
    {
        private readonly IDoctorService _doctorService;
        private readonly IMapper _mapper;

        public DoctorsController(IDoctorService doctorService, IMapper mapper)
        {
            _doctorService = doctorService;
            _mapper = mapper;
		}

        /// <summary>
        /// Retrieves top-rated doctors.
        /// </summary>
        /// <param name="count">Number of top doctors to return (default is 3).</param>
        [HttpGet("top-rated")]
        public async Task<ActionResult<IReadOnlyList<DoctorSearchResultDto>>> GetTopRated([FromQuery] int count = 3)
        {
            var topDoctors = await _doctorService.GetTopRatedDoctorsAsync(count);
            return Ok(topDoctors);
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

        /// <summary>
        /// Retrieves all doctors.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetAll()
        {
            var spec = new BaseSpecification<Doctor>(s => true);
            spec.Includes.Add(d => d.Availabilities);
            spec.Includes.Add(d => d.Specialization);

            var doctors = await _doctorService.GetAllDoctorsAsync(spec);
            var result = _mapper.Map<IReadOnlyList<DoctorDto>>(doctors);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new doctor (Admin only).
        /// </summary>
        /// <param name="dto">The doctor details to create.</param>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DoctorDto>> Create(AdminCreateDoctorDto dto)
        {
            var createdDoctor = await _doctorService.CreateDoctorAsync(dto);
            var result = _mapper.Map<DoctorDto>(createdDoctor);
            return CreatedAtAction(nameof(GetById), new { id = createdDoctor.Id }, result);
        }

        /// <summary>
        /// Updates an existing doctor.
        /// </summary>
        /// <param name="id">The ID of the doctor to update.</param>
        /// <param name="doctor">The updated doctor details.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Doctor doctor)
        {
            if (id != doctor.Id)
                return BadRequest("ID mismatch");

            await _doctorService.UpdateDoctorAsync(doctor);
            return NoContent();
        }

        /// <summary>
        /// Deletes a doctor.
        /// </summary>
        /// <param name="id">The ID of the doctor to delete.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _doctorService.DeleteDoctorAsync(id);
            return NoContent();
        }


        /// <summary>
        /// Retrieves a doctor by ID.
        /// </summary>
        /// <param name="id">The ID of the doctor.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Doctor>> GetById(int id)
        {

            return Ok();
        }

    }
}
