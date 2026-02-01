using MedLink.Domain.Entities.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Specifications.Chat
{
    public class AppointmentWithDoctorSpec : BaseSpecification<Appointment>
    {
        public AppointmentWithDoctorSpec(int appointmentId)
            : base(a => a.Id == appointmentId)
        {
            Includes.Add(a => a.Doctor); // الدكتور
            Includes.Add(a => a.Schedule); // الجدول
        }
    }
}
