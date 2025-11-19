namespace ShowplaceAPI.Models.DTOModles
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public string Message { get; set; }
    }
}
