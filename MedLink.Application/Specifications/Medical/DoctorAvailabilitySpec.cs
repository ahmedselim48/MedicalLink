using MedLink.Application.Specifications;
using MedLink.Domain.Entities.Appointments;
using System;
using System.Linq.Expressions;

namespace MedLink.Application.Specifications.Medical
{
    public class DoctorAvailabilitySpec : BaseSpecification<DoctorAvailability>
    {
        // Constructor for fetching ALL slots for a doctor 
        public DoctorAvailabilitySpec(int doctorId)
            : base(x => x.DoctorId == doctorId && !x.IsDeleted)
        {
            AddOrderBy(x => x.Date);
            AddOrderBy(x => x.StartTime);
        }

        public DoctorAvailabilitySpec(int doctorId, DateTime? date, bool isAvailable)
            : base(x => 
                x.DoctorId == doctorId && !x.IsDeleted &&
                (!date.HasValue || x.Date.Date == date.Value.Date) &&
                (!isAvailable || !x.IsBooked)
            )
        {
            AddOrderBy(x => x.Date);
            AddOrderBy(x => x.StartTime);
        }
    }
}
