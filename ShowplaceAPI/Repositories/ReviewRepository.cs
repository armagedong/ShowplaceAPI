using Microsoft.EntityFrameworkCore;
using ShowplaceAPI.Models;
using ShowplaceAPI.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ShowplaceAPI.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Review> GetByIdAsync(int id)
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return await _context.Reviews
                .Include(r => r.Landmark)
                .FirstOrDefaultAsync(r => r.Id == id);
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.Landmark)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> FindAsync(Expression<Func<Review, bool>> predicate)
        {
            return await _context.Reviews
                .Include(r => r.Landmark)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Review> AddAsync(Review entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            _context.Reviews.Add(entity);
            await _context.SaveChangesAsync();

            // Загружаем связанные данные
            await _context.Entry(entity)
                .Reference(r => r.Landmark)
                .LoadAsync();

            return entity;
        }

        public async Task<Review> UpdateAsync(Review entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id) => await _context.Reviews.AnyAsync(e => e.Id == id);
        public async Task<int> CountAsync() => await _context.Reviews.CountAsync();
        public async Task<int> CountAsync(Expression<Func<Review, bool>> predicate) => await _context.Reviews.CountAsync(predicate);

        public async Task<IEnumerable<Review>> GetReviewsWithLandmarksAsync()
        {
            return await _context.Reviews
                .Include(r => r.Landmark)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByLandmarkAsync(int landmarkId)
        {
            return await _context.Reviews
                .Where(r => r.LandmarkId == landmarkId)
                .Include(r => r.Landmark)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByRatingAsync(int minRating)
        {
            return await _context.Reviews
                .Where(r => r.Rating >= minRating)
                .Include(r => r.Landmark)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 10)
        {
            return await _context.Reviews
                .Include(r => r.Landmark)
                .OrderByDescending(r => r.CreatedDate)
                .Take(count)
                .ToListAsync();
        }
       
        public async Task<double> GetAverageRatingForLandmarkAsync(int landmarkId)
        {
            return await _context.Reviews
                .Where(r => r.LandmarkId == landmarkId)
                .AverageAsync(r => r.Rating);
        }
        //завтра доделать
    }
}
