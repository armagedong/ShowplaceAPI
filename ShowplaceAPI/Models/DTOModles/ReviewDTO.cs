namespace ShowplaceAPI.Models.DTOModles
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Author { get; set; } = string.Empty;
        public string? AuthorEmail { get; set; }
        public int LandmarkId { get; set; }
        public string LandmarkName { get; set; } = string.Empty;
    }
}
