using AutoMapper;
using golden_fork.core.DTOs;
using golden_fork.Core.IServices;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace golden_fork.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class AppUserController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAppUserService _appUserService;

        public AppUserController(IUnitOfWork unitOfWork, IMapper mapper, IAppUserService appUserService)
            : base(unitOfWork, mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _appUserService = appUserService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _appUserService.RegisterAsync(dto);

            if (result.success)
            {
                return Ok(new
                {
                    success = true,
                    message = result.message,
                    userId = result.userId
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.message
            });
        }

        // In your AuthController or UserController
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value; // or however you store username in claims
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            return Ok(new
            {
                Id = int.Parse(userId),
                Email = email,
                Username = username,
                Role = role
            });
        }


        [Authorize]
        [HttpGet("debug/userinfo")]
        public IActionResult DebugUserInfo()
        {
            var user = User;
            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Username = user.Identity?.Name,
                Claims = claims,
                RoleId = user.FindFirst("RoleId")?.Value,
                Role = user.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _appUserService.LoginAsync(dto);

            if (result.success && result.user != null)
            {
                var token = _appUserService.GenerateJwtToken(result.user);

                // RETURN TOKEN IN BODY — NO COOKIE
                return Ok(new
                {
                    success = true,
                    message = result.message,
                    token = token,  // ← frontend will store this
                    user = new
                    {
                        id = result.user.Id,
                        username = result.user.Username,
                        email = result.user.Email,
                        role = result.user.Role?.Name
                    }
                });
            }

            return Unauthorized(new
            {
                success = false,
                message = result.message
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Expire the cookie by setting it with a past expiration date
            Response.Cookies.Append("GoldenForkAuth", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expire immediately
            });

            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize] 
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Assuming BaseController has a method to get current user ID from JWT
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == userId,
                include: q => q.Include(u => u.Role));

            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            return Ok(new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    role = user.Role?.Name,
                    createdAt = user.CreatedAt
                }
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == userId);

            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            // Update allowed fields
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            // Check if email is being changed and if it's already taken
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var existingEmail = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                    u => u.Email == dto.Email && u.Id != userId);

                if (existingEmail != null)
                    return BadRequest(new { success = false, message = "Email already taken" });

                user.Email = dto.Email;
            }

            await _unitOfWork.UserRepository.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Profile updated successfully"
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Id == userId);

            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                return BadRequest(new { success = false, message = "Current password is incorrect" });

            // Hash new password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.Password = hashedPassword;

            await _unitOfWork.UserRepository.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Password changed successfully"
            });
        }

        // Helper method to get current user ID from claims
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                return userId;
            return null;
        }
    }

    // DTO classes for the additional endpoints
    public class UpdateProfileRequest
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}