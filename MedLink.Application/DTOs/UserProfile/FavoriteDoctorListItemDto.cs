namespace MedLink.Application.DTOs.Favorites;

public class FavoriteDoctorListItemDto
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;
    public string DoctorImageUrl { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;

    public double Rating { get; set; }
    public int ReviewsCount { get; set; }

    public string Location { get; set; } = string.Empty;
    public decimal Fee { get; set; }

    public bool IsOnlineAvailable { get; set; }
}
