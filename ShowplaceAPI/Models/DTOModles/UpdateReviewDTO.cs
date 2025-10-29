namespace ShowplaceAPI.Models.DTOModles
{
    public class UpdateReviewDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Author { get; set; } = string.Empty;
        public string? AuthorEmail { get; set; }
    }
}
