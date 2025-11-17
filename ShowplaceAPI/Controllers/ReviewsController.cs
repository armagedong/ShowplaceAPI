using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowplaceAPI.Models;
using ShowplaceAPI.Models.DTOModles;
using ShowplaceAPI.Repositories.Interfaces;

namespace ShowplaceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(IReviewRepository reviewRepository,
        ILandmarkRepository landmarkRepository, IMapper mapper) : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository = reviewRepository;
        private readonly ILandmarkRepository _landmarkRepository = landmarkRepository;
        private readonly IMapper _mapper = mapper;

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviews()
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsWithLandmarksAsync();
                var reviewsDTOs = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
                return Ok(reviewsDTOs);
            }
            catch (Exception)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = "Error retrieving reviews from database",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDTO>> GetReview(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Review with ID {id} not found.",
                    Instance = $"/api/reviews/{id}"
                });
            }

            var reviewDTO = _mapper.Map<ReviewDTO>(review);
            return Ok(reviewDTO);
        }

        // POST: api/Reviews
        [HttpPost]
        public async Task<ActionResult<ReviewDTO>> PostReview(CreateReviewDto createReviewDto)
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

            // Проверяем существование достопримечательности
            if (!await _landmarkRepository.ExistsAsync(createReviewDto.LandmarkId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = $"Landmark with ID {createReviewDto.LandmarkId} not found.",
                    Instance = HttpContext.Request.Path
                });
            }

            // Проверяем валидность рейтинга
            if (createReviewDto.Rating < 1 || createReviewDto.Rating > 5)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "Rating must be between 1 and 5.",
                    Instance = HttpContext.Request.Path
                });
            }

            var review = _mapper.Map<Review>(createReviewDto);
            var createdReview = await _reviewRepository.AddAsync(review);
            var reviewDTO = _mapper.Map<ReviewDTO>(createdReview);

            return CreatedAtAction("GetReview", new { id = reviewDTO.Id }, reviewDTO);
        }

        // PUT: api/Reviews/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, UpdateReviewDTO updateReviewDto)
        {
            if (id != updateReviewDto.Id)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "ID in URL does not match ID in body.",
                    Instance = HttpContext.Request.Path
                });
            }

            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Review with ID {id} not found.",
                    Instance = $"/api/reviews/{id}"
                });
            }

            // Проверяем валидность рейтинга
            if (updateReviewDto.Rating < 1 || updateReviewDto.Rating > 5)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "Rating must be between 1 and 5.",
                    Instance = HttpContext.Request.Path
                });
            }

            _mapper.Map(updateReviewDto, review);

            try
            {
                await _reviewRepository.UpdateAsync(review);
            }
            catch (Exception)
            {
                if (!await ReviewExists(id))
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

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Review with ID {id} not found.",
                    Instance = $"/api/reviews/{id}"
                });
            }

            await _reviewRepository.DeleteAsync(id);

            return NoContent();
        }

        // GET: api/Reviews/landmark/5
        [HttpGet("landmark/{landmarkId}")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviewsByLandmark(int landmarkId)
        {
            if (!await _landmarkRepository.ExistsAsync(landmarkId))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"Landmark with ID {landmarkId} not found.",
                    Instance = $"/api/reviews/landmark/{landmarkId}"
                });
            }

            var reviews = await _reviewRepository.GetReviewsByLandmarkAsync(landmarkId);
            var reviewDTOs = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);

            return Ok(reviewDTOs);
        }

        // GET: api/Reviews/rating/5
        [HttpGet("rating/{minRating}")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviewsByMinRating(int minRating)
        {
            if (minRating < 1 || minRating > 5)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "Rating must be between 1 and 5.",
                    Instance = HttpContext.Request.Path
                });
            }

            var reviews = await _reviewRepository.GetReviewsByRatingAsync(minRating);
            var reviewDTOs = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);

            return Ok(reviewDTOs);
        }

        // GET: api/Reviews/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetRecentReviews()
        {
            var reviews = await _reviewRepository.GetRecentReviewsAsync(10);
            var reviewDTOs = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);

            return Ok(reviewDTOs);
        }

        private async Task<bool> ReviewExists(int id)
        {
            return await _reviewRepository.ExistsAsync(id);
        }
    }
}
