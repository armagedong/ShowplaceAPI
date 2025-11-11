using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowplaceAPI.Models;
using ShowplaceAPI.Models.DTOModles;
using ShowplaceAPI.Repositories.Interfaces;

namespace ShowplaceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandmarksController : ControllerBase
    {
        private readonly ILandmarkRepository _landmarkRepository;
        private readonly IReviewRepository _reviewRepository; // Добавим для работы с отзывами

        public LandmarksController(ILandmarkRepository landmarkRepository, IReviewRepository reviewRepository)
        {
            _landmarkRepository = landmarkRepository;
            _reviewRepository = reviewRepository;  
        }

        // GET: api/Landmarks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> GetLandmarks()
        {
            var landmarks = await _landmarkRepository.GetLandmarksWithReviewsAsync();
            var landMarkDTOs = landmarks.Select(l => new LandmarkDTO
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Location = l.Location,
                ImageUrl = l.ImageUrl,
                CreatedDate = l.CreatedDate,
                ReviewsCount = l.Reviews.Count,
                AverageRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : null
            });

            return Ok(landMarkDTOs);
        }

        // GET: api/Landmarks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LandmarkDTO>> GetLandmark(int id)
        {
            var landmark = await _landmarkRepository.GetLandmarkWithReviewsAsync(id);
            if (landmark == null) 
            {
                return NotFound(); // Добавлен return
            }

            var landmarkDTO = new LandmarkDTO
            {
                Id = landmark.Id,
                Name = landmark.Name,
                Description = landmark.Description,
                Location = landmark.Location,
                ImageUrl = landmark.ImageUrl,
                CreatedDate = landmark.CreatedDate,
                ReviewsCount = landmark.Reviews.Count,
                AverageRating = landmark.Reviews.Any() ? landmark.Reviews.Average(r => r.Rating) : null
            };

            return landmarkDTO;
        }

        // POST: api/Landmarks
        [HttpPost]
        public async Task<ActionResult<LandmarkDTO>> PostLandmark(CreateLandmarkDto createLandmarkDto)
        {
            var landmark = new Landmark
            {
                Name = createLandmarkDto.Name,
                Description = createLandmarkDto.Description,
                Location = createLandmarkDto.Location,
                ImageUrl = createLandmarkDto.ImageUrl
                // CreatedDate устанавливается в репозитории
            };

            var createdLandmark = await _landmarkRepository.AddAsync(landmark); // Используем репозиторий

            var landmarkDto = new LandmarkDTO
            {
                Id = createdLandmark.Id,
                Name = createdLandmark.Name,
                Description = createdLandmark.Description,
                Location = createdLandmark.Location,
                ImageUrl = createdLandmark.ImageUrl,
                CreatedDate = createdLandmark.CreatedDate,
                ReviewsCount = 0,
                AverageRating = null
            };

            return CreatedAtAction("GetLandmark", new { id = landmarkDto.Id }, landmarkDto);
        }

        // PUT: api/Landmarks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLandmark(int id, UpdateLandmarkDTO updateLandmarkDto)
        {
            var landmark = await _landmarkRepository.GetByIdAsync(id); // Используем репозиторий
            if (landmark == null) 
            {
                return NotFound(); // Добавлен return
            }

            landmark.Name = updateLandmarkDto.Name;
            landmark.Description = updateLandmarkDto.Description;
            landmark.Location = updateLandmarkDto.Location;
            landmark.ImageUrl = updateLandmarkDto.ImageUrl;

            try
            {
                await _landmarkRepository.UpdateAsync(landmark); // Используем репозиторий
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LandmarkExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Landmarks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLandmark(int id)
        {
            var landmark = await _landmarkRepository.GetByIdAsync(id); // Используем репозиторий
            if (landmark == null)
            {
                return NotFound();
            }

            await _landmarkRepository.DeleteAsync(id); // Используем репозиторий

            return NoContent();
        }

        // GET: api/Landmarks/5/Reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetLandmarkReviews(int id)
        {
            // Проверяем существование достопримечательности
            if (!await _landmarkRepository.ExistsAsync(id))
            {
                return NotFound();
            }

            var reviews = await _reviewRepository.GetReviewsByLandmarkAsync(id); // Используем репозиторий

            var reviewDtos = reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                Title = r.Title,
                Content = r.Content,
                Rating = r.Rating,
                CreatedDate = r.CreatedDate,
                Author = r.Author,
                AuthorEmail = r.AuthorEmail,
                LandmarkId = r.LandmarkId,
                LandmarkName = r.Landmark?.Name ?? "Unknown" // Безопасное обращение
            });

            return Ok(reviewDtos);
        }

        // GET: api/Landmarks/search?name=это
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> SearchLandmarks([FromQuery] string name)
        {
            var landmarks = await _landmarkRepository.SearchLandmarksAsync(name); // Используем репозиторий

            var landmarkDtos = landmarks.Select(l => new LandmarkDTO
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Location = l.Location,
                ImageUrl = l.ImageUrl,
                CreatedDate = l.CreatedDate,
                ReviewsCount = l.Reviews.Count,
                AverageRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : null
            });

            return Ok(landmarkDtos);
        }

        // GET: api/Landmarks/top-rated
        [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> GetTopRatedLandmarks()
        {
            var landmarks = await _landmarkRepository.GetTopRatedLandmarksAsync(10); // Используем репозиторий

            var landmarkDtos = landmarks.Select(l => new LandmarkDTO
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Location = l.Location,
                ImageUrl = l.ImageUrl,
                CreatedDate = l.CreatedDate,
                ReviewsCount = l.Reviews.Count,
                AverageRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : null
            });

            return Ok(landmarkDtos);
        }

        private async Task<bool> LandmarkExists(int id) // Сделали метод асинхронным
        {
            return await _landmarkRepository.ExistsAsync(id); // Используем репозиторий
        }
    }
}
