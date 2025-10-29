using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowplaceAPI.Models;
using ShowplaceAPI.Models.DTOModles;

namespace ShowplaceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users
            .Include(u => u.Reviews)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedDate = u.CreatedDate,
                    ReviewsCount = u.Reviews.Count
                })
                .ToListAsync();

            return users;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Reviews)
            .Where(u => u.Id == id)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedDate = u.CreatedDate,
                    ReviewsCount = u.Reviews.Count
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(CreateUserDTO createUserDto)
        {
            // Проверяем уникальность email и username
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                return BadRequest("User with this email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                return BadRequest("User with this username already exists");
            }

            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedDate = user.CreatedDate,
                ReviewsCount = 0
            };

            return CreatedAtAction("GetUser", new { id = user.Id }, userDto);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UpdateUserDTO updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Проверяем уникальность email и username (исключая текущего пользователя)
            if (await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id))
            {
                return BadRequest("User with this email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username && u.Id != id))
            {
                return BadRequest("User with this username already exists");
            }

            user.Username = updateUserDto.Username;
            user.Email = updateUserDto.Email;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Users/5/reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetUserReviews(int id)
        {
            var user = await _context.Users
                .Include(u => u.Reviews)
                .ThenInclude(r => r.Landmark)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var reviewDtos = user.Reviews.Select(r => new ReviewDTO
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
            });

            return Ok(reviewDtos);
        }

        // GET: api/Users/by-email?email=test@example.com
        [HttpGet("by-email")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail([FromQuery] string email)
        {
            var user = await _context.Users
                .Include(u => u.Reviews)
                .Where(u => u.Email == email)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedDate = u.CreatedDate,
                    ReviewsCount = u.Reviews.Count
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
