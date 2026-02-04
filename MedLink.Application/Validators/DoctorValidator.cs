using MedLink.Domain.Entities.Medical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using System.Threading.Tasks;

namespace MedLink.Application.Validators
{
    public class DoctorValidator : AbstractValidator<Doctor>
    {
        public DoctorValidator()
        {

            RuleFor(x => x.Name)
                 .NotEmpty().WithMessage("Doctor name is required.")
                 .MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
                 .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

           
            RuleFor(x => x.SpecialtyId)
                .GreaterThan(0).WithMessage("Please select a valid specialization.");

           
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Consultation price cannot be negative.");

           
            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.");

           
            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Please select a valid gender.");

           
            RuleFor(x => x.Location)
                .NotNull().WithMessage("Clinic location on the map is required.");

           
            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters.");

           
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Clinic address is required.");
        }
    }
}
