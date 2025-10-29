namespace ShowplaceAPI.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string Author { get; set; } = string.Empty;
        public string? AuthorEmail { get; set; }
        public int LandmarkId { get; set; }
        public Landmark Landmark { get; set; } = null!;
    }
}