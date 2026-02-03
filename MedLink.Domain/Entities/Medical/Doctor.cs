using MedLink.Domain.Common;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.User;
using MedLink.Domain.Identity;
using MedLink.Domain.Enums;
using NetTopologySuite.Geometries;

namespace MedLink.Domain.Entities.Medical;

public class Doctor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; } = null!;

    public int SpecialtyId { get; set; }
    public Specialization? Specialization { get; set; } = null!;

    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? ClinicDetails { get; set; }

    public string City { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public bool IsDeleted { get; set; } = false;

    /// Geographic location of doctor's clinic (SRID 4326 = WGS84)
    public Point Location { get; set; }

    public string? Address { get; set; } = string.Empty;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public int ConsultationFee { get; set; } = 400;
}

