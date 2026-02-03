using MedLink.Domain.Enums;

namespace MedLink.Application.DTOs.Doctors
{
    public class DoctorSearchParams
    {
        private const int MaxPageSize = 50;

        public int PageIndex { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? Keyword { get; set; }
        public string? City { get; set; }
        public Gender? Gender { get; set; }
        public int? SpecialtyId { get; set; }

        public bool AvailableOnDate { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double RadiusInKm { get; set; } = 10;
        public DateTime SearchDate { get; set; } = DateTime.Today;
    }
}


