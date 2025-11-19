using Microsoft.EntityFrameworkCore;

namespace ShowplaceAPI.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Landmark> Landmarks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
            });
        }
    }
}
