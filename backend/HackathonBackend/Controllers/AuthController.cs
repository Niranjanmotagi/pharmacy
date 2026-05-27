using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using HackathonBackend.Data;
using HackathonBackend.Models;

namespace HackathonBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtOptions _jwt;

        public AuthController(AppDbContext context, JwtOptions jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // ===== DTOs (inline, kept compatible with existing frontend) =====

        public class RegisterDto
        {
            [Required(ErrorMessage = "Username is required.")]
            [RegularExpression(
                "^[A-Za-z]{4,20}$",
                ErrorMessage = "Username must be 4-20 letters only (no numbers or spaces).")]
            public string Username { get; set; } = "";

            [Required(ErrorMessage = "Password is required.")]
            [RegularExpression(
                "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,64}$",
                ErrorMessage = "Password must be 8+ chars with uppercase, lowercase, number and special character.")]
            public string Password { get; set; } = "";

            [RegularExpression("^(Customer|Admin)$", ErrorMessage = "Role must be Customer or Admin.")]
            public string Role { get; set; } = "Customer";

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Email format is invalid.")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "First name is required.")]
            [RegularExpression(
                "^[A-Za-z]{1,30}$",
                ErrorMessage = "First name must contain only letters (max 30).")]
            public string FirstName { get; set; } = "";

            [Required(ErrorMessage = "Last name is required.")]
            [RegularExpression(
                "^[A-Za-z]{1,30}$",
                ErrorMessage = "Last name must contain only letters (max 30).")]
            public string LastName { get; set; } = "";

            [Required(ErrorMessage = "Phone number is required.")]
            [RegularExpression("^\\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
            public string PhoneNumber { get; set; } = "";
        }

        public class LoginDto
        {
            [Required(ErrorMessage = "Username is required.")]
            public string Username { get; set; } = "";

            [Required(ErrorMessage = "Password is required.")]
            public string Password { get; set; } = "";
        }

        // ===== Endpoints =====

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            // DataAnnotations are auto-validated; this is a defense-in-depth check.
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (_context.Users.Any(u => u.Username == dto.Username))
                return BadRequest(new { message = "Username already taken" });

            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already registered" });

            var role = dto.Role == "Admin" ? "Admin" : "Customer";

            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                Role = role,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                LoyaltyPoints = 0
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Registered successfully", userId = user.Id });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var user = _context.Users.FirstOrDefault(
                u => u.Username == dto.Username && u.Password == dto.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                username = user.Username,
                role = user.Role,
                userId = user.Id
            });
        }
    }
}
