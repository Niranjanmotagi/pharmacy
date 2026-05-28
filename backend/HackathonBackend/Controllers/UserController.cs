using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HackathonBackend.Data;
using HackathonBackend.Models;

namespace HackathonBackend.Controllers
{
    /// <summary>
    /// Profile endpoints for the currently authenticated user.
    /// All endpoints require a valid JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ===== DTOs =====

        public class UpdateProfileDto
        {
            [Required(ErrorMessage = "First name is required.")]
            [RegularExpression("^[A-Za-z]{1,30}$",
                ErrorMessage = "First name must contain only letters (max 30).")]
            public string FirstName { get; set; } = "";

            [Required(ErrorMessage = "Last name is required.")]
            [RegularExpression("^[A-Za-z]{1,30}$",
                ErrorMessage = "Last name must contain only letters (max 30).")]
            public string LastName { get; set; } = "";

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Email format is invalid.")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Phone number is required.")]
            [RegularExpression("^\\d{10}$",
                ErrorMessage = "Phone must be exactly 10 digits.")]
            public string PhoneNumber { get; set; } = "";
        }

        public class ChangePasswordDto
        {
            [Required(ErrorMessage = "Current password is required.")]
            public string CurrentPassword { get; set; } = "";

            [Required(ErrorMessage = "New password is required.")]
            [RegularExpression(
                "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,64}$",
                ErrorMessage = "New password must be 8+ chars with uppercase, lowercase, number and special character.")]
            public string NewPassword { get; set; } = "";
        }

        // ===== Helpers =====

        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr ?? "0");
        }

        // ===== Endpoints =====

        /// <summary>
        /// Get the current user's profile and derived stats.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            int userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == userId);
            var pendingOrders = await _context.Orders.CountAsync(o =>
                o.UserId == userId &&
                o.Status != null &&
                o.Status.ToLower().Contains("pending"));
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == userId &&
                            (o.Status ?? "").ToLower() != "rejected")
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;

            // "Active since" — we don't store a CreatedAt column to avoid a
            // schema change. Derive it from the lowest order date if any, else
            // null. Front-end gracefully handles the missing case.
            DateTime? activeSince = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderBy(o => o.OrderDate)
                .Select(o => (DateTime?)o.OrderDate)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                loyaltyPoints = user.LoyaltyPoints,
                totalOrders,
                pendingOrders,
                totalSpent,
                activeSince
            });
        }

        /// <summary>
        /// Update the current user's editable profile fields.
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            int userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Reject email collisions
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase) &&
                await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId))
            {
                return BadRequest(new { message = "Email already in use." });
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated",
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber
            });
        }

        /// <summary>
        /// Change the current user's password.
        /// </summary>
        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            int userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.Password != dto.CurrentPassword)
            {
                return BadRequest(new { message = "Current password is incorrect." });
            }

            if (dto.NewPassword == dto.CurrentPassword)
            {
                return BadRequest(new { message = "New password must be different from current." });
            }

            user.Password = dto.NewPassword;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
