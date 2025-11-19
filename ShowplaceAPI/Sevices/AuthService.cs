using Microsoft.IdentityModel.Tokens;
using ShowplaceAPI.Models.DTOModles;
using ShowplaceAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ShowplaceAPI.Sevices
{
    public class AuthService : IAuthService
    { 
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerDto)
        {
            // Проверка существования пользователя
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new ArgumentException("User with this email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                throw new ArgumentException("User with this username already exists");
            }

            // Хеширование пароля
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Создание пользователя
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Генерация токена
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresInMinutes"])),
                Message = "User registered successfully"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresInMinutes"])),
                Message = "Login successful"
            };
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

