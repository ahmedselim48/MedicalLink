using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.DTOs.Doctors
{
    public class SpecializationsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DoctorSimpleDto> Doctors { get; set; }
    }
}
