using MedLink.Application.DTOs.Medical;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Domain.Entities.Medical;

namespace MedLink.Application.Interfaces.Services
{
    public interface ISpecializationService
    {
        Task<IReadOnlyList<SpecializationDto>> GetAllSpecializationsAsync(int? count = null);
        Task<Specialization?> GetSpecializationByIdAsync(int id);
        Task<IReadOnlyList<Specialization>> GetAllSpecializationsAsync(ISpecification<Specialization>? spec = null);
        Task<Specialization> CreateSpecializationAsync(Specialization specialization);
        Task UpdateSpecializationAsync(Specialization specialization);
        Task DeleteSpecializationAsync(int id);
    }
}
