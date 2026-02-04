using MedLink.Domain.Entities.Content;
using MedLink.Domain.Entities.Medical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Application.Interfaces.Services;

namespace MedLink.Application.Services
{
    public class FAQService : IFAQ
    {
        private readonly IUnitOfWork _unitOfWork;

        public FAQService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<FAQ> CreateQuestionAsync(FAQ faq)
        {
          
            var repo = _unitOfWork.Repository<FAQ>();
            var isOrderTaken = await repo.AnyAsync(f => f.DisplayOrder == faq.DisplayOrder);

            if (isOrderTaken)
            {
               
                throw new Exception("This display order is already assigned to another question.");
            }

           
            await repo.AddAsync(faq);
            await _unitOfWork.Complete();

            return faq;
        }

        public async Task SubmitAnswerAsync(int faqId, string answer, int userProfileId)
        {
            var repo = _unitOfWork.Repository<FAQ>();
            var faq = await repo.GetByIdAsync(faqId);

            if (faq == null) throw new Exception("Question not found");

           
            faq.Answer = answer;
            faq.AnsweredByProfileId = userProfileId;
            faq.IsActive = true; 

             repo.Update(faq);
            await _unitOfWork.Complete();
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

        public Task<bool> IsDisplayOrderUniqueAsync(int displayOrder, int? excludeId = null)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateQuestionAsync(FAQ faq)
        {
            var repo = _unitOfWork.Repository<FAQ>();

          
            var isOrderTaken = await repo.AnyAsync(f => f.DisplayOrder == faq.DisplayOrder && f.Id != faq.Id);

            if (isOrderTaken)
            {
                throw new Exception("This display order is already assigned to another question.");
            }

             repo.Update(faq);
            await _unitOfWork.Complete();
        }
    }
}
