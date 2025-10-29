namespace ShowplaceAPI.Models.DTOModles
{
    public class UpdateLandmarkDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
