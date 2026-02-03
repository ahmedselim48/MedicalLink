using System.Linq.Expressions;
using MedLink.Application.DTOs.Doctors;
using MedLink.Domain.Entities.Medical;
using NetTopologySuite.Geometries;

namespace MedLink.Application.Specifications.Medical
{
    /// <summary>
    /// Lightweight specification for doctor search listings.
    /// </summary>
    /// <remarks>
    /// Uses a hybrid spatial search strategy (bounding box + distance) for optimal performance.
    /// Bridges API latitude/longitude inputs with domain-level <see cref="Point"/> location handling.
    /// </remarks>
    public class DoctorSearchSpec : BaseSpecification<Doctor>
    {
        private const double MinRadiusKm = 1.0;
        private const double KilometersPerDegree = 111.0;
        private const double MetersPerKilometer = 1000.0;
        private const int Srid = 4326;

        public DoctorSearchSpec(DoctorSearchParams searchParams)
            : base(BuildCriteria(searchParams))
        {
            AddIncludes(d => d.Specialization);
            AddIncludes(d => d.Reviews);
            AddIncludes(d => d.Availabilities);
            AddOrderBy(d => d.Name);

            var pageIndex = Math.Max(searchParams.PageIndex, 1);
            var pageSize = Math.Max(searchParams.PageSize, 1);
            ApplyPagination((pageIndex - 1) * pageSize, pageSize);
        }

        private static Expression<Func<Doctor, bool>> BuildCriteria(DoctorSearchParams p)
        {
            var keyword = p.Keyword?.Trim();
            var hasKeyword = !string.IsNullOrEmpty(keyword);

            var city = p.City?.Trim();
            var hasCity = !string.IsNullOrEmpty(city);

            var hasLocation = p.Latitude.HasValue && p.Longitude.HasValue;

            Point? searchPoint = null;
            double radiusMeters = 0;
            double minLat = 0, maxLat = 0, minLng = 0, maxLng = 0;

            if (hasLocation)
            {
                var radiusKm = Math.Max(p.RadiusInKm, MinRadiusKm);
                radiusMeters = radiusKm * MetersPerKilometer;
                searchPoint = CreatePoint(p.Longitude!.Value, p.Latitude!.Value);

                var radiusDeg = radiusKm / KilometersPerDegree;
                minLat = p.Latitude!.Value - radiusDeg;
                maxLat = p.Latitude.Value + radiusDeg;
                minLng = p.Longitude!.Value - radiusDeg;
                maxLng = p.Longitude.Value + radiusDeg;
            }

            var filterByAvailability = p.AvailableOnDate; // This line is ambiguous due to duplicate property definitions.
            var searchDateStart = p.SearchDate.Date;
            var searchDateEnd = searchDateStart.AddDays(1);

            return d =>
                // Location filter: City > Coordinates > None
                (hasCity
                    ? (d.City != null && d.City == city)
                    : hasLocation
                        ? (d.Location != null &&
                           d.Location.Y >= minLat && d.Location.Y <= maxLat &&
                           d.Location.X >= minLng && d.Location.X <= maxLng &&
                           d.Location.Distance(searchPoint) <= radiusMeters)
                        : true)

                // Keyword in name or specialty
                && (!hasKeyword || (
                    (d.Name != null && d.Name.Contains(keyword!)) ||
                    (d.Specialization != null && d.Specialization.Name != null &&
                     d.Specialization.Name.Contains(keyword!))))

                // Filters
                && (!p.Gender.HasValue || d.Gender == p.Gender)
                && (!p.SpecialtyId.HasValue || d.SpecialtyId == p.SpecialtyId)
                && (!filterByAvailability || d.Availabilities.Any(a =>
                       a.Date >= searchDateStart && a.Date < searchDateEnd));
        }

        private static Point CreatePoint(double longitude, double latitude) =>
            new Point(longitude, latitude) { SRID = Srid };
    }
}
