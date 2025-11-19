using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShowplaceAPI.Models.DTOModles;
using ShowplaceAPI.Sevices;
using System.Threading.Tasks.Dataflow;

namespace ShowplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registerDto">User registration data</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return CreatedAtAction(nameof(Register), result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="loginDto">User login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
