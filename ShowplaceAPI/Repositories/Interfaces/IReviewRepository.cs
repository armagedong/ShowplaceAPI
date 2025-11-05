using ShowplaceAPI.Models;

namespace ShowplaceAPI.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsWithLandmarksAsync();
        Task<IEnumerable<Review>> GetReviewsByLandmarkAsync(int landmarkId);
        Task<IEnumerable<Review>> GetReviewsByRatingAsync(int minRating);
        Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 10);
        Task<double> GetAverageRatingForLandmarkAsync(int landmarkId);
    }
}
