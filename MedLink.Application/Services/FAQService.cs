using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Domain.Entities.Content;

namespace MedLink.Application.Services
{
    public class FAQService : IFAQ
    {
        private readonly IUnitOfWork _unitOfWork;

        public FAQService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<FAQ> CreateQuestionAsync(FAQ q)
        {
            var repo = _unitOfWork.Repository<FAQ>();
            await repo.AddAsync(q);
            await _unitOfWork.Complete();
            return q;
        }



        public async Task<IReadOnlyList<FAQ>> GetAllQuestionsAsync(ISpecification<FAQ>? spec)
        {
            var repo = _unitOfWork.Repository<FAQ>();
            return spec != null
                ? await repo.GetAllWithSpecAsync(spec)
                : await repo.GetAllAsync();
        }


        //public async Task DeleteQuestionAsync(Guid id)
        //{
        //    var repo = _unitOfWork.Repository<FAQ>();
        //    var entity =  repo.GetByIdAsync(id);
        //    if (entity != null)
        //    {
        //     awai   repo.Delete(entity);
        //         _unitOfWork.Complete();
        //    }
        //}

        public Task<FAQ?> GetQuestionByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<FAQ>();
            return repo.GetByIdAsync(id);
        }

        public Task<FAQ?> GetQuestionByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateQuestionAsync(FAQ Faq)
        {
            var repo = _unitOfWork.Repository<FAQ>();
            repo.Update(Faq);
            await _unitOfWork.Complete();
        }
    }
}
