using MedLink.Domain.Common;

namespace MedLink.Domain.Entities.Content
{
    public class Language : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
