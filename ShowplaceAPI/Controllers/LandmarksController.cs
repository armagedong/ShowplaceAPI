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

        public LandmarksController(ILandmarkRepository landmarkRepository)
        {
            _landmarkRepository = landmarkRepository;
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
            if (landmark == null) NotFound();

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
                ImageUrl = createLandmarkDto.ImageUrl,
                CreatedDate = DateTime.UtcNow
            };

            _context.Landmarks.Add(landmark);
            await _context.SaveChangesAsync();

            var landmarkDto = new LandmarkDTO
            {
                Id = landmark.Id,
                Name = landmark.Name,
                Description = landmark.Description,
                Location = landmark.Location,
                ImageUrl = landmark.ImageUrl,
                CreatedDate = landmark.CreatedDate,
                ReviewsCount = 0,
                AverageRating = null
            };

            return CreatedAtAction("GetLandmark", new { id = landmark.Id }, landmarkDto);
        }

        // PUT: api/Landmarks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLandmark(int id, UpdateLandmarkDTO updateLandmarkDto)
        {
            var landmark = await _landmarkRepository.GetLandmarkWithReviewsAsync(id);
            if (landmark == null) NotFound();

            landmark.Name = updateLandmarkDto.Name;
            landmark.Description = updateLandmarkDto.Description;
            landmark.Location = updateLandmarkDto.Location;
            landmark.ImageUrl = updateLandmarkDto.ImageUrl;

            try
            {
                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LandmarkExists(id))
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
            var landmark = await _context.Landmarks.FindAsync(id);
            if (landmark == null)
            {
                return NotFound();
            }

            _context.Landmarks.Remove(landmark);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Landmarks/5/Reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetLandmarkReviews(int id)
        {
            var landmark = await _context.Landmarks
                .Include(l => l.Reviews)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (landmark == null)
            {
                return NotFound();
            }

            var reviewDtos = landmark.Reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                Title = r.Title,
                Content = r.Content,
                Rating = r.Rating,
                CreatedDate = r.CreatedDate,
                Author = r.Author,
                AuthorEmail = r.AuthorEmail,
                LandmarkId = r.LandmarkId,
                LandmarkName = landmark.Name
            });

            return Ok(reviewDtos);
        }

        // GET: api/Landmarks/search?name=это
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> SearchLandmarks([FromQuery] string name)
        {
            var landmarks = await _context.Landmarks
                .Where(l => l.Name.ToLower().Contains(name.ToLower()))
                .Include(l => l.Reviews)
                .Select(l => new LandmarkDTO
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Location = l.Location,
                    ImageUrl = l.ImageUrl,
                    CreatedDate = l.CreatedDate,
                    ReviewsCount = l.Reviews.Count,
                    AverageRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : null
                })
                .ToListAsync();

            return landmarks;
        }

        // GET: api/Landmarks/top-rated
        [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> GetTopRatedLandmarks()
        {
            var landmarks = await _context.Landmarks
                .Include(l => l.Reviews)
                .Where(l => l.Reviews.Any())
                .Select(l => new LandmarkDTO
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Location = l.Location,
                    ImageUrl = l.ImageUrl,
                    CreatedDate = l.CreatedDate,
                    ReviewsCount = l.Reviews.Count,
                    AverageRating = l.Reviews.Average(r => r.Rating)
                })
                .OrderByDescending(l => l.AverageRating)
                .ThenByDescending(l => l.ReviewsCount)
                .Take(10)
                .ToListAsync();

            return landmarks;
        }

        private bool LandmarkExists(int id)
        {
            return _context.Landmarks.Any(e => e.Id == id);
        }
    }
}
