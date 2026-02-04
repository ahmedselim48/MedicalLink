using FluentValidation;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Domain.Entities.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Validators
{
    public class FAQValidator : AbstractValidator<FAQ>
    {
        private readonly IUnitOfWork _unitOfWork;
        public FAQValidator()
        {
            // 1. Question validation
            RuleFor(x => x.Question)
                .NotEmpty().WithMessage("Question text is required.")
                .MaximumLength(500).WithMessage("Question cannot exceed 500 characters.");

            // 2. Display Order validation
            RuleFor(x => x.DisplayOrder)
                .GreaterThan(0).WithMessage("Display order must be a positive number greater than zero.");




            // 4. Answerer tracking validation
            // Logic: If there is an answer, we must know which UserProfile provided it.
            RuleFor(x => x.AnsweredByProfileId)
                .NotNull()
                .When(x => !string.IsNullOrEmpty(x.Answer))
                .WithMessage("The profile of the user providing the answer must be recorded.");
        }
    }
}
