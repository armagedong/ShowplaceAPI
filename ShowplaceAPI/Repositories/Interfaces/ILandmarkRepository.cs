using ShowplaceAPI.Models;

namespace ShowplaceAPI.Repositories.Interfaces
{
    public interface ILandmarkRepository : IRepository<Landmark>
    {
        Task<IEnumerable<Landmark>> GetLandmarksWithReviewsAsync();
        Task<Landmark> GetLandmarkWithReviewsAsync(int id);
        Task<IEnumerable<Landmark>> SearchLandmarksAsync(string searchTerm);
        Task<IEnumerable<Landmark>> GetTopRatedLandmarksAsync(int count = 10);
        Task<double?> GetAverageRatingAsync(int landmarkId);
        Task<int> GetReviewsCountAsync(int landmarkId);
    }
}
