namespace ShowplaceAPI.Models.DTOModles
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int ReviewsCount { get; set; }
    }
}
