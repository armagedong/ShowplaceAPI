using System.ComponentModel.DataAnnotations;

namespace ShowplaceAPI.Models.DTOModles
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
