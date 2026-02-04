using FluentValidation;
using MedLink.Domain.Entities.Medical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Validators
{
    public class SpecializationValidator : AbstractValidator<Specialization>
    {
        public SpecializationValidator()
        {
           
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Specialization name is required.")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

           
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        }
    
}
}
