using MedLink.Application.Interfaces.Specifications;
using MedLink.Domain.Entities.Content;

namespace MedLink.Application.Interfaces.Services
{
    public interface IFAQ
    {
        Task<FAQ?> GetQuestionByIdAsync(Guid id);
        Task<IReadOnlyList<FAQ>> GetAllQuestionsAsync(ISpecification<FAQ>? spec = null);
        Task<FAQ> CreateQuestionAsync(FAQ Faq);
        Task UpdateQuestionAsync(FAQ Faq);
        // Task DeleteQuestionAsync(Guid id);
    }
}
