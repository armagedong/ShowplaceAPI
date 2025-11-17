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
    public class UsersController(IUserRepository userRepository, 
        IMapper mapper) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IMapper _mapper = mapper;

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _userRepository.GetUsersWithReviewsAsync();
            var userDTOs = _mapper.Map<IEnumerable<UserDTO>>(users);
            return Ok(userDTOs);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"User with ID {id} not found.",
                    Instance = $"/api/users/{id}"
                });
            }

            var userDTO = _mapper.Map<UserDTO>(user);
            return userDTO;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(CreateUserDTO createUserDto)
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

            // Проверяем уникальность email
            if (await _userRepository.EmailExistsAsync(createUserDto.Email))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "User with this email already exists.",
                    Instance = HttpContext.Request.Path
                });
            }

            // Проверяем уникальность username
            if (await _userRepository.UsernameExistsAsync(createUserDto.Username))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "User with this username already exists.",
                    Instance = HttpContext.Request.Path
                });
            }

            var user = _mapper.Map<User>(createUserDto);
            var createdUser = await _userRepository.AddAsync(user);
            var userDTO = _mapper.Map<UserDTO>(createdUser);

            return CreatedAtAction("GetUser", new { id = userDTO.Id }, userDTO);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UpdateUserDTO updateUserDto)
        {
            if (id != updateUserDto.Id)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "ID in URL does not match ID in body.",
                    Instance = HttpContext.Request.Path
                });
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"User with ID {id} not found.",
                    Instance = $"/api/users/{id}"
                });
            }

            // Проверяем уникальность email (исключая текущего пользователя)
            if (await _userRepository.EmailExistsAsync(updateUserDto.Email) &&
                (await _userRepository.GetUserByEmailAsync(updateUserDto.Email))?.Id != id)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "User with this email already exists.",
                    Instance = HttpContext.Request.Path
                });
            }

            // Проверяем уникальность username (исключая текущего пользователя)
            if (await _userRepository.UsernameExistsAsync(updateUserDto.Username) &&
                (await _userRepository.GetUserByUsernameAsync(updateUserDto.Username))?.Id != id)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = 400,
                    Detail = "User with this username already exists.",
                    Instance = HttpContext.Request.Path
                });
            }

            _mapper.Map(updateUserDto, user);

            try
            {
                await _userRepository.UpdateAsync(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id))
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
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"User with ID {id} not found.",
                    Instance = $"/api/users/{id}"
                });
            }

            await _userRepository.DeleteAsync(id);

            return NoContent();
        }

        // GET: api/Users/5/reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetUserReviews(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"User with ID {id} not found.",
                    Instance = $"/api/users/{id}/reviews"
                });
            }

            var reviews = user.Reviews;
            var reviewDTOs = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);

            return Ok(reviewDTOs);
        }

        // GET: api/Users/by-email?email=test@example.com
        [HttpGet("by-email")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"User with email {email} not found.",
                    Instance = HttpContext.Request.Path
                });
            }

            var userDTO = _mapper.Map<UserDTO>(user);
            return userDTO;
        }

        // GET: api/Users/by-username?username=testuser
        [HttpGet("by-username")]
        public async Task<ActionResult<UserDTO>> GetUserByUsername([FromQuery] string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = $"User with username {username} not found.",
                    Instance = HttpContext.Request.Path
                });
            }

            var userDTO = _mapper.Map<UserDTO>(user);
            return userDTO;
        }

        private async Task<bool> UserExists(int id)
        {
            return await _userRepository.ExistsAsync(id);
        }
    }
}
