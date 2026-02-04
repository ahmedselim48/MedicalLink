using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.Content;

namespace MedLink.Application.Services
{
    public class AboutService : IAboutService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AboutService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<About> AddAboutAsync(About about)
        {
            await _unitOfWork.Repository<About>().AddAsync(about);
            await _unitOfWork.Complete();
            return about;
        }

        public async Task DeleteAboutAsync(int id)
        {
            var repo = _unitOfWork.Repository<About>();
            var entity = await repo.GetByIdAsync(id);
            if (entity != null)
            {
                repo.Delete(entity);
                await _unitOfWork.Complete();
            }
        }

        public async Task<About?> GetAboutByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<About>();
            return await repo.GetByIdAsync(id);
        }

        public async Task UpdateAboutAsync(About about)
        {
            var repo = _unitOfWork.Repository<About>();
            repo.Update(about);
            await _unitOfWork.Complete();
        }
    }
}
