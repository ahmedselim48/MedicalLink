using MedLink.Domain.Common;

namespace MedLink.Domain.Entities.Medical;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public object Include(Func<object, object> value)
    {
        throw new NotImplementedException();
    }
}
