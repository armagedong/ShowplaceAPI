using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShowplaceAPI.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
