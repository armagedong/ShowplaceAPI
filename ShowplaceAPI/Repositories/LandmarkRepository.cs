using Microsoft.EntityFrameworkCore;
using ShowplaceAPI.Models;
using ShowplaceAPI.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ShowplaceAPI.Repositories
{
    public class LandmarkRepository : ILandmarkRepository
    {
        private readonly AppDbContext _context;

        public LandmarkRepository(AppDbContext context)
        {
            _context = context;
        }

#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        public async Task<Landmark> GetByIdAsync(int id) => await _context.Landmarks.FindAsync(id);
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.

        public async Task<IEnumerable<Landmark>> GetAllAsync() => await _context.Landmarks.ToListAsync();
        public async Task<IEnumerable<Landmark>> FindAsync(Expression<Func<Landmark, bool>> predicate) => await _context.Landmarks.Where(predicate).ToListAsync();

        public async Task<Landmark> AddAsync(Landmark entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            _context.Landmarks.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Landmark> UpdateAsync(Landmark entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var landmark = await _context.Landmarks.FindAsync(id);
            if (landmark != null)
            {
                _context.Landmarks.Remove(landmark);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id) => await _context.Landmarks.AnyAsync(e => e.Id == id);
        public async Task<int> CountAsync() => await _context.Landmarks.CountAsync();
        public async Task<int> CountAsync(Expression<Func<Landmark, bool>> predicate) => await _context.Landmarks.CountAsync(predicate);

        public async Task<IEnumerable<Landmark>> GetLandmarksWithReviewsAsync()
        {
            return await _context.Landmarks
                .Include(l => l.Reviews)
                .ToListAsync();
        }

        public async Task<Landmark> GetLandmarkWithReviewsAsync(int id)
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return await _context.Landmarks
                .Include(l => l.Reviews)
                .FirstOrDefaultAsync(l => l.Id == id);
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        public async Task<IEnumerable<Landmark>> SearchLandmarksAsync(string searchTerm)
        {
            return await _context.Landmarks
                .Where(l => l.Name.ToLower().Contains(searchTerm.ToLower()) ||
                           l.Description.ToLower().Contains(searchTerm.ToLower()) ||
                           l.Location.ToLower().Contains(searchTerm.ToLower()))
                .Include(l => l.Reviews)
                .ToListAsync();
        }

        public async Task<IEnumerable<Landmark>> GetTopRatedLandmarksAsync(int count = 10)
        {
            return await _context.Landmarks
                .Include(l => l.Reviews)
                .Where(l => l.Reviews.Any())
                .Select(l => new
                {
                    Landmark = l,
                    AverageRating = l.Reviews.Average(r => r.Rating)
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.Landmark.Reviews.Count)
                .Take(count)
                .Select(x => x.Landmark)
                .ToListAsync();
        }

        public async Task<double?> GetAverageRatingAsync(int landmarkId)
        {
            var landmark = await _context.Landmarks
                .Include(l => l.Reviews)
                .FirstOrDefaultAsync(l => l.Id == landmarkId);

            return landmark?.Reviews.Any() == true ? landmark.Reviews.Average(r => r.Rating) : null;
        }

        public async Task<int> GetReviewsCountAsync(int landmarkId)
        {
            return await _context.Reviews
                .Where(r => r.LandmarkId == landmarkId)
                .CountAsync();
        }
    }
}
