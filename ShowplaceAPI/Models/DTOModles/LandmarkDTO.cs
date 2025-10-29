namespace ShowplaceAPI.Models.DTOModles
{
    public class LandmarkDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ReviewsCount { get; set; }
        public double? AverageRating { get; set; }
    }
}
