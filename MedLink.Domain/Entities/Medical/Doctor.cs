using MedLink.Domain.Common;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.User;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NetTopologySuite.Geometries;

namespace MedLink.Domain.Entities.Medical;

public class Doctor : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int SpecialtyId { get; set; }
    public Specialization? Specialization { get; set; } = null!;

    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? ClinicDetails { get; set; }

    public string City { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public bool IsDeleted { get; set; }= false;

    /// Geographic location of doctor's clinic (SRID 4326 = WGS84)
    /// 
    [ValidateNever]
    public Point Location { get; set; } = null!;

    public string? Address { get; set; } = string.Empty;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

