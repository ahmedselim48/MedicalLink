using AutoMapper;
using MedLink.Application.DTOs.Medical;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Application.Specifications.Medical;
using MedLink.Domain.Entities.Medical;

namespace MedLink.Application.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly IGenericRepository<Specialization> _specializationRepo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public SpecializationService(IGenericRepository<Specialization> specializationRepo, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _specializationRepo = specializationRepo;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<SpecializationDto>> GetAllSpecializationsAsync(int? count = null)
        {
            var spec = new SpecialtyListSpec(count);
            var specializations = await _specializationRepo.GetAllWithSpecAsync(spec);

            return _mapper.Map<IReadOnlyList<SpecializationDto>>(specializations);
        }
        public async Task<Specialization> CreateSpecializationAsync(Specialization specialization)
        {

            var repo = _unitOfWork.Repository<Specialization>();
            await repo.AddAsync(specialization);
            await _unitOfWork.Complete();
            return specialization;
        }


        public async Task<IReadOnlyList<Specialization>> GetAllSpecializationsAsync(ISpecification<Specialization>? spec = null)
        {
            var repo = _unitOfWork.Repository<Specialization>();
            return spec != null
                ? await repo.GetAllWithSpecAsync(spec)
                : await repo.GetAllAsync();
        }


        public async Task<Specialization?> GetSpecializationByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Specialization>();
            return await repo.GetByIdAsync(id);
        }


        public async Task UpdateSpecializationAsync(Specialization specialization)
        {
            var repo = _unitOfWork.Repository<Specialization>();
            repo.Update(specialization);
            await _unitOfWork.Complete();
        }


        public async Task DeleteSpecializationAsync(int id)
        {
            var repo = _unitOfWork.Repository<Specialization>();
            var entity = await repo.GetByIdAsync(id);
            if (entity != null)
            {
                repo.Delete(entity);
                await _unitOfWork.Complete();
            }
        }

    }
}
