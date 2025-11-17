using AutoMapper;
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
    public class LandmarksController(ILandmarkRepository landmarkRepository,
        IReviewRepository reviewRepository, IMapper mapper) : ControllerBase
    {
        private readonly ILandmarkRepository _landmarkRepository = landmarkRepository;
        private readonly IReviewRepository _reviewRepository = reviewRepository;
        private readonly IMapper _mapper = mapper;

        // GET: api/Landmarks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> GetLandmarks()
        {
            try
            {
                var landmarks = await _landmarkRepository.GetLandmarksWithReviewsAsync();
                var landmarkDTOs = _mapper.Map<IEnumerable<LandmarkDTO>>(landmarks);
                return Ok(landmarkDTOs);
            }
            catch (Exception)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = "Error retrieving landmarks from database",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        // GET: api/Landmarks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LandmarkDTO>> GetLandmark(int id)
        {
            var landmark = await _landmarkRepository.GetLandmarkWithReviewsAsync(id);
            if (landmark == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Landmark with ID {id} not found.",
                    Instance = $"/api/landmarks/{id}"
                });
            }

            var landmarkDTO = _mapper.Map<LandmarkDTO>(landmark);
            return Ok(landmarkDTO);
        }

        // POST: api/Landmarks
        [HttpPost]
        public async Task<ActionResult<LandmarkDTO>> PostLandmark(CreateLandmarkDto createLandmarkDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "Validation errors occurred",
                    Instance = HttpContext.Request.Path,
                    Extensions = { ["errors"] = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )}
                });
            }

            var landmark = _mapper.Map<Landmark>(createLandmarkDto);
            var createdLandmark = await _landmarkRepository.AddAsync(landmark);
            var landmarkDTO = _mapper.Map<LandmarkDTO>(createdLandmark);

            return CreatedAtAction("GetLandmark", new { id = landmarkDTO.Id }, landmarkDTO);
        }

        // PUT: api/Landmarks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLandmark(int id, UpdateLandmarkDTO updateLandmarkDto)
        {
            if (id != updateLandmarkDto.Id)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "ID in URL does not match ID in body.",
                    Instance = HttpContext.Request.Path
                });
            }

            var landmark = await _landmarkRepository.GetByIdAsync(id);
            if (landmark == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Landmark with ID {id} not found.",
                    Instance = $"/api/landmarks/{id}"
                });
            }

            _mapper.Map(updateLandmarkDto, landmark);

            try
            {
                await _landmarkRepository.UpdateAsync(landmark);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LandmarkExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Landmarks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLandmark(int id)
        {
            var landmark = await _landmarkRepository.GetByIdAsync(id);
            if (landmark == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Landmark with ID {id} not found.",
                    Instance = $"/api/landmarks/{id}"
                });
            }

            await _landmarkRepository.DeleteAsync(id);

            return NoContent();
        }

        // GET: api/Landmarks/5/Reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetLandmarkReviews(int id)
        {
            if (!await _landmarkRepository.ExistsAsync(id))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Landmark with ID {id} not found.",
                    Instance = $"/api/landmarks/{id}/reviews"
                });
            }

            var reviews = await _reviewRepository.GetReviewsByLandmarkAsync(id);
            var reviewDTOs = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);

            return Ok(reviewDTOs);
        }
        // GET: api/Landmarks/search?name=это
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> SearchLandmarks([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "")
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "Search term cannot be empty.",
                    Instance = HttpContext.Request.Path
                });
            }

            try
            {
                var landmarks = await _landmarkRepository.SearchLandmarksAsync(name);
                var landmarkDTOs = _mapper.Map<IEnumerable<LandmarkDTO>>(landmarks);

                return Ok(landmarkDTOs);
            }
            catch (Exception)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = "Error searching landmarks",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        // GET: api/Landmarks/top-rated
        [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> GetTopRatedLandmarks()
        {
            var landmarks = await _landmarkRepository.GetTopRatedLandmarksAsync(10);
            var landmarkDTOs = _mapper.Map<IEnumerable<LandmarkDTO>>(landmarks);

            return Ok(landmarkDTOs);
        }

        // GET: api/Landmarks/most-reviewed
        [HttpGet("most-reviewed")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> GetMostReviewedLandmarks()
        {
            try
            {
                var landmarks = await _landmarkRepository.GetLandmarksWithReviewsAsync();
                var mostReviewedLandmarks = landmarks
                    .Where(l => l.Reviews.Any())
                    .OrderByDescending(l => l.Reviews.Count)
                    .Take(10)
                    .ToList();

                var landmarkDTOs = _mapper.Map<IEnumerable<LandmarkDTO>>(mostReviewedLandmarks);
                return Ok(landmarkDTOs);
            }
            catch (Exception)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = "Error retrieving most reviewed landmarks",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        // GET: api/Landmarks/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<LandmarkDTO>>> GetRecentLandmarks()
        {
            try
            {
                var landmarks = await _landmarkRepository.GetLandmarksWithReviewsAsync();
                var recentLandmarks = landmarks
                    .OrderByDescending(l => l.CreatedDate)
                    .Take(10)
                    .ToList();

                var landmarkDTOs = _mapper.Map<IEnumerable<LandmarkDTO>>(recentLandmarks);
                return Ok(landmarkDTOs);
            }
            catch (Exception)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = "Error retrieving recent landmarks",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        // GET: api/Landmarks/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            try
            {
                var totalLandmarks = await _landmarkRepository.CountAsync();
                var landmarksWithReviews = await _landmarkRepository.FindAsync(l => l.Reviews.Any());
                var totalReviews = landmarksWithReviews.Sum(l => l.Reviews.Count);
                var averageRating = landmarksWithReviews.Any()
                    ? landmarksWithReviews.Average(l => l.Reviews.Average(r => r.Rating))
                    : 0;

                var statistics = new
                {
                    TotalLandmarks = totalLandmarks,
                    LandmarksWithReviews = landmarksWithReviews.Count(),
                    TotalReviews = totalReviews,
                    AverageRating = Math.Round(averageRating, 2),
                    MostReviewedLandmark = landmarksWithReviews
                        .OrderByDescending(l => l.Reviews.Count)
                        .FirstOrDefault()?.Name ?? "No reviews yet"
                };

                return Ok(statistics);
            }
            catch (Exception)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = "Error retrieving statistics",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        private async Task<bool> LandmarkExists(int id)
        {
            return await _landmarkRepository.ExistsAsync(id);
        }
    }
}
