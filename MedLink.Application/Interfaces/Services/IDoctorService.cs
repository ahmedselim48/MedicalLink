using MedLink.Application.DTOs.Doctors;
using MedLink.Application.DTOs.Identity;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Domain.Entities.Medical;

namespace MedLink.Application.Interfaces.Services
{
    public interface IDoctorService
    {
        Task<IReadOnlyList<DoctorSearchResultDto>> SearchDoctorsAsync(DoctorSearchParams searchParams);
        Task<IReadOnlyList<DoctorSearchResultDto>> GetTopRatedDoctorsAsync(int count);
        Task<Doctor?> GetDoctorByIdAsync(int id);
        Task<IReadOnlyList<Doctor>> GetAllDoctorsAsync(ISpecification<Doctor>? spec = null);
        Task<Doctor> AddDoctorAsync(Doctor doctor);
        Task<Doctor> CreateDoctorAsync(AdminCreateDoctorDto dto);
        Task UpdateDoctorAsync(Doctor doctor);
        Task DeleteDoctorAsync(int id);
    }
}