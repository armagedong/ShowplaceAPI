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
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository; 
        private readonly IUserRepository _userRepository;
        private readonly ILandmarkRepository _landmarkRepository;

        public ReviewsController(IReviewRepository reviewRepository, IUserRepository userRepository, ILandmarkRepository landmarkRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _landmarkRepository = landmarkRepository;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviews()
        { 
            var reviews = await _reviewRepository.GetAllAsync();

            var reviewsDTOs = reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                Title = r.Title,
                Content = r.Content,
                Rating = r.Rating,
                CreatedDate = r.CreatedDate,
                Author = r.Author,
                AuthorEmail = r.AuthorEmail,
                LandmarkId = r.LandmarkId,
                LandmarkName = r.Landmark.Name,
            });
            return Ok(reviewsDTOs);
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDTO>> GetReview(int id)
        {
            //var review = await _context.Reviews
            //    .Include(r => r.Landmark)
            //.Where(r => r.Id == id)
            //    .Select(r => new ReviewDTO
            //    {
            //        Id = r.Id,
            //        Title = r.Title,
            //        Content = r.Content,
            //        Rating = r.Rating,
            //        CreatedDate = r.CreatedDate,
            //        Author = r.Author,
            //        AuthorEmail = r.AuthorEmail,
            //        LandmarkId = r.LandmarkId,
            //        LandmarkName = r.Landmark.Name
            //    })
            //    .FirstOrDefaultAsync();

            //if (review == null)
            //{
            //    return NotFound();
            //}

            //return review;

            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) NotFound();

            var reviewDTO = new ReviewDTO
            {
                Id = review.Id,
                Title = review.Title,
                Content = review.Content,
                Rating = review.Rating,
                CreatedDate = review.CreatedDate,
                Author = review.Author,
                AuthorEmail = review.AuthorEmail,
                LandmarkId = review.LandmarkId,
                LandmarkName = review.Landmark.Name,
            };
        }

        // POST: api/Reviews
        [HttpPost]
        public async Task<ActionResult<ReviewDTO>> PostReview(CreateReviewDto createReviewDto)
        {
            // Проверяем существование достопримечательности
            var landmark = await _landmarkRepository.GetLandmarkWithReviewsAsync(createReviewDto.LandmarkId);
            if (landmark == null)
            {
                return BadRequest("Landmark not found");
            }

            // Проверяем валидность рейтинга
            if (createReviewDto.Rating < 1 || createReviewDto.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5");
            }

            var review = new Review
            {
                Title = createReviewDto.Title,
                Content = createReviewDto.Content,
                Rating = createReviewDto.Rating,
                Author = createReviewDto.Author,
                AuthorEmail = createReviewDto.AuthorEmail,
                LandmarkId = createReviewDto.LandmarkId,
             };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var reviewDto = new ReviewDTO
            {
                Id = review.Id,
                Title = review.Title,
                Content = review.Content,
                Rating = review.Rating,
                CreatedDate = review.CreatedDate,
                Author = review.Author,
                AuthorEmail = review.AuthorEmail,
                LandmarkId = review.LandmarkId,
                LandmarkName = landmark.Name
            };

            return CreatedAtAction("GetReview", new { id = review.Id }, reviewDto);
        }

        // PUT: api/Reviews/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, UpdateReviewDTO updateReviewDto)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            // Проверяем валидность рейтинга
            if (updateReviewDto.Rating < 1 || updateReviewDto.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5");
            }

            review.Title = updateReviewDto.Title;
            review.Content = updateReviewDto.Content;
            review.Rating = updateReviewDto.Rating;
            review.Author = updateReviewDto.Author;
            review.AuthorEmail = updateReviewDto.AuthorEmail;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
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
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Reviews/landmark/5
        [HttpGet("landmark/{landmarkId}")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviewsByLandmark(int landmarkId)
        {
            var landmark = await _context.Landmarks.FindAsync(landmarkId);
            if (landmark == null)
            {
                return NotFound("Landmark not found");
            }

            var reviews = await _context.Reviews
                .Where(r => r.LandmarkId == landmarkId)
                .Include(r => r.Landmark)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Content = r.Content,
                    Rating = r.Rating,
                    CreatedDate = r.CreatedDate,
                    Author = r.Author,
                    AuthorEmail = r.AuthorEmail,
                    LandmarkId = r.LandmarkId,
                    LandmarkName = r.Landmark.Name
                })
                .ToListAsync();

            return reviews;
        }

        // GET: api/Reviews/rating/5
        [HttpGet("rating/{minRating}")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetReviewsByMinRating(int minRating)
        {
            if (minRating < 1 || minRating > 5)
            {
                return BadRequest("Rating must be between 1 and 5");
            }

            var reviews = await _context.Reviews
                .Where(r => r.Rating >= minRating)
                .Include(r => r.Landmark)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedDate)
                .Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Content = r.Content,
                    Rating = r.Rating,
                    CreatedDate = r.CreatedDate,
                    Author = r.Author,
                    AuthorEmail = r.AuthorEmail,
                    LandmarkId = r.LandmarkId,
                    LandmarkName = r.Landmark.Name
                })
                .ToListAsync();

            return reviews;
        }

        // GET: api/Reviews/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetRecentReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Landmark)
                .OrderByDescending(r => r.CreatedDate)
            .Take(10)
                .Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Content = r.Content,
                    Rating = r.Rating,
                    CreatedDate = r.CreatedDate,
                    Author = r.Author,
                    AuthorEmail = r.AuthorEmail,
                    LandmarkId = r.LandmarkId,
                    LandmarkName = r.Landmark.Name
                })
                .ToListAsync();

            return reviews;
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
